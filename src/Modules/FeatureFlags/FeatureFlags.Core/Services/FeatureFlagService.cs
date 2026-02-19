using System.Linq.Expressions;
using System.Text.Json;
using FeatureFlags.Contracts;
using FeatureFlags.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SaasKit.Infrastructure.Api;
using SaasKit.Infrastructure.Caching;
using SaasKit.Infrastructure.Persistence;
using SaasKit.Infrastructure.Sse;
using SaasKit.SharedKernel.Api;
using SaasKit.SharedKernel.Interfaces;
using SaasKit.SharedKernel.Models;

namespace FeatureFlags.Core.Services;

public class FeatureFlagService : IFeatureFlagService
{
    private readonly AppDbContext _db;
    private readonly IRedisCacheService _cache;
    private readonly ISseNotifier _sseNotifier;
    private readonly IClock _clock;
    private readonly ILogger<FeatureFlagService> _logger;

    private const string CachePrefix = "feature_flag";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(1);

    public FeatureFlagService(AppDbContext db, IRedisCacheService cache, ISseNotifier sseNotifier, IClock clock, ILogger<FeatureFlagService> logger)
    {
        _db = db;
        _cache = cache;
        _sseNotifier = sseNotifier;
        _clock = clock;
        _logger = logger;
    }

    public async Task<PagedList<FeatureFlagDto>> GetFlagsAsync(QueryParameters qp, CancellationToken ct = default)
    {
        var filters = new Dictionary<string, Expression<Func<FeatureFlag, object>>> { ["isEnabled"] = x => x.IsEnabled, ["name"] = x => x.Name };
        var query = _db.Set<FeatureFlag>().AsNoTracking().ApplyFilters(qp.Filters, filters).ApplySort(qp.GetSortFields(), new Dictionary<string, Expression<Func<FeatureFlag, object>>> { ["name"] = x => x.Name, ["createdAt"] = x => x.CreatedAt });
        return await query.Select(x => new FeatureFlagDto(x.Id, x.Name, x.Description, x.IsEnabled, x.Percentage, x.CreatedAt)).ToPagedListAsync(qp, ct);
    }

    public async Task<Result<FeatureFlagDto>> GetFlagByNameAsync(string name, CancellationToken ct = default)
    {
        var flag = await _db.Set<FeatureFlag>().AsNoTracking().FirstOrDefaultAsync(x => x.Name == name, ct);
        if (flag is null) return Result<FeatureFlagDto>.NotFound("Flag not found");
        return Result<FeatureFlagDto>.Success(new FeatureFlagDto(flag.Id, flag.Name, flag.Description, flag.IsEnabled, flag.Percentage, flag.CreatedAt));
    }

    public async Task<Result<FeatureFlagDto>> CreateFlagAsync(CreateFeatureFlagRequest request, CancellationToken ct = default)
    {
        if (await _db.Set<FeatureFlag>().AnyAsync(x => x.Name == request.Name, ct))
            return Result<FeatureFlagDto>.Conflict($"Flag '{request.Name}' already exists");

        var flag = new FeatureFlag { Id = Guid.NewGuid(), Name = request.Name, Description = request.Description, IsEnabled = request.IsEnabled, Percentage = request.Percentage, EnabledAt = request.IsEnabled ? _clock.UtcNow : null };
        _db.Set<FeatureFlag>().Add(flag);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Created feature flag {FlagName}", flag.Name);
        return Result<FeatureFlagDto>.Success(new FeatureFlagDto(flag.Id, flag.Name, flag.Description, flag.IsEnabled, flag.Percentage, flag.CreatedAt));
    }

    public async Task<Result<FeatureFlagDto>> UpdateFlagAsync(Guid flagId, UpdateFeatureFlagRequest request, CancellationToken ct = default)
    {
        var flag = await _db.Set<FeatureFlag>().FindAsync([flagId], ct);
        if (flag is null) return Result<FeatureFlagDto>.NotFound("Flag not found");

        if (request.Description is not null) flag.Description = request.Description;
        if (request.Percentage.HasValue) flag.Percentage = request.Percentage;
        if (request.IsEnabled.HasValue && request.IsEnabled.Value != flag.IsEnabled)
        {
            flag.IsEnabled = request.IsEnabled.Value;
            if (flag.IsEnabled) flag.EnabledAt = _clock.UtcNow; else flag.DisabledAt = _clock.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
        await _cache.RemoveAsync($"{CachePrefix}:{flag.Name}", ct);
        await _sseNotifier.BroadcastAsync("feature_flag.changed", new { flagName = flag.Name, isEnabled = flag.IsEnabled }, ct);
        _logger.LogInformation("Updated feature flag {FlagName}", flag.Name);
        return Result<FeatureFlagDto>.Success(new FeatureFlagDto(flag.Id, flag.Name, flag.Description, flag.IsEnabled, flag.Percentage, flag.CreatedAt));
    }

    public async Task<Result<bool>> DeleteFlagAsync(Guid flagId, CancellationToken ct = default)
    {
        var flag = await _db.Set<FeatureFlag>().FindAsync([flagId], ct);
        if (flag is null) return Result<bool>.NotFound("Flag not found");
        _db.Set<FeatureFlag>().Remove(flag);
        await _db.SaveChangesAsync(ct);
        await _cache.RemoveAsync($"{CachePrefix}:{flag.Name}", ct);
        return Result<bool>.Success(true);
    }

    public async Task<EvaluationResult> EvaluateFlagAsync(string name, Guid tenantId, string? planSlug, CancellationToken ct = default)
    {
        var cacheKey = $"{CachePrefix}:{name}";
        var cached = await _cache.GetAsync<FeatureFlag>(cacheKey, ct);
        
        FeatureFlag? flag = cached;
        if (flag is null)
        {
            flag = await _db.Set<FeatureFlag>().AsNoTracking().Include(x => x.Filters).FirstOrDefaultAsync(x => x.Name == name, ct);
            if (flag is not null) await _cache.SetAsync(cacheKey, flag, CacheDuration, ct);
        }

        if (flag is null) return new EvaluationResult(name, false, "Flag not found");
        if (!flag.IsEnabled) return new EvaluationResult(name, false, "Flag is disabled");

        // Check allowed tenant IDs
        if (!string.IsNullOrEmpty(flag.AllowedTenantIds))
        {
            var allowedIds = JsonSerializer.Deserialize<List<string>>(flag.AllowedTenantIds);
            if (allowedIds?.Contains(tenantId.ToString()) == true)
                return new EvaluationResult(name, true, "Tenant allowed");
        }

        // Check allowed plans
        if (!string.IsNullOrEmpty(flag.AllowedPlans) && !string.IsNullOrEmpty(planSlug))
        {
            var allowedPlans = JsonSerializer.Deserialize<List<string>>(flag.AllowedPlans);
            if (allowedPlans?.Contains(planSlug) == true)
                return new EvaluationResult(name, true, "Plan allowed");
        }

        // Percentage rollout (deterministic based on tenant ID)
        if (flag.Percentage.HasValue && flag.Percentage > 0)
        {
            var hash = Math.Abs(tenantId.GetHashCode()) % 100;
            if (hash < flag.Percentage) return new EvaluationResult(name, true, $"Percentage rollout ({flag.Percentage}%)");
        }

        return new EvaluationResult(name, flag.Percentage == null || flag.Percentage == 100, "Default");
    }
}
