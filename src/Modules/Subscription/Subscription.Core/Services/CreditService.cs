using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SaasKit.Infrastructure.Api;
using SaasKit.Infrastructure.Caching;
using SaasKit.Infrastructure.Persistence;
using SaasKit.SharedKernel.Api;
using SaasKit.SharedKernel.Interfaces;
using SaasKit.SharedKernel.Models;
using Subscription.Contracts;
using Subscription.Contracts.DTOs;
using Subscription.Core.Entities;

namespace Subscription.Core.Services;

/// <summary>
/// Service for managing credit ledger.
/// Uses append-only ledger pattern with Redis balance cache.
/// </summary>
public class CreditService : ICreditService
{
    private readonly AppDbContext _db;
    private readonly IRedisCacheService _cache;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly ILogger<CreditService> _logger;

    private const string BalanceCacheKeyPrefix = "tenant:credits:balance";
    private static readonly TimeSpan BalanceCacheDuration = TimeSpan.FromMinutes(5);

    private static readonly Dictionary<string, Expression<Func<Credit, object>>> CreditFilters = new()
    {
        ["type"] = x => x.Type,
        ["createdAt"] = x => x.CreatedAt
    };

    private static readonly Dictionary<string, Expression<Func<Credit, object>>> CreditSortable = new()
    {
        ["createdAt"] = x => x.CreatedAt,
        ["amount"] = x => x.Amount
    };

    public CreditService(
        AppDbContext db,
        IRedisCacheService cache,
        ICurrentUser currentUser,
        IClock clock,
        ILogger<CreditService> logger)
    {
        _db = db;
        _cache = cache;
        _currentUser = currentUser;
        _clock = clock;
        _logger = logger;
    }

    public async Task<CreditBalanceDto> GetBalanceAsync(Guid tenantId, CancellationToken ct = default)
    {
        var cacheKey = $"{BalanceCacheKeyPrefix}:{tenantId}";

        var cached = await _cache.GetAsync<CreditBalanceDto>(cacheKey, ct);
        if (cached is not null)
        {
            return cached;
        }

        // Get current balance from latest entry
        var latestEntry = await _db.Set<Credit>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

        var balance = latestEntry?.Balance ?? 0;

        // Calculate expiring credits
        var thirtyDaysFromNow = _clock.UtcNow.AddDays(30);
        var expiringCredits = await _db.Set<Credit>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId &&
                        x.Amount > 0 &&
                        x.ExpiresAt != null &&
                        x.ExpiresAt <= thirtyDaysFromNow &&
                        x.ExpiresAt > _clock.UtcNow)
            .SumAsync(x => x.Amount, ct);

        var nextExpiration = await _db.Set<Credit>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId &&
                        x.Amount > 0 &&
                        x.ExpiresAt != null &&
                        x.ExpiresAt > _clock.UtcNow)
            .OrderBy(x => x.ExpiresAt)
            .Select(x => x.ExpiresAt)
            .FirstOrDefaultAsync(ct);

        var result = new CreditBalanceDto
        {
            TenantId = tenantId,
            Balance = balance,
            ExpiringWithin30Days = expiringCredits,
            NextExpiration = nextExpiration
        };

        await _cache.SetAsync(cacheKey, result, BalanceCacheDuration, ct);

        return result;
    }

    public async Task<PagedList<CreditDto>> GetHistoryAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<Credit>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ApplyFilters(qp.Filters, CreditFilters)
            .ApplySort(qp.GetSortFields(), CreditSortable);

        return await query
            .Select(x => MapToDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<CreditDto>> AddCreditsAsync(Guid tenantId, AddCreditsRequest request, CancellationToken ct = default)
    {
        if (request.Amount <= 0)
            return Result<CreditDto>.ValidationError("Amount must be positive");

        var validTypes = new[] { "purchase", "bonus", "refund" };
        if (!validTypes.Contains(request.Type))
            return Result<CreditDto>.ValidationError($"Invalid type. Must be one of: {string.Join(", ", validTypes)}");

        // Get current balance
        var currentBalance = await GetCurrentBalanceAsync(tenantId, ct);
        var newBalance = currentBalance + request.Amount;

        var credit = new Credit
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Type = request.Type,
            Amount = request.Amount,
            Balance = newBalance,
            Description = request.Description,
            UserId = _currentUser.UserId,
            ExpiresAt = request.ExpiresAt
        };

        _db.Set<Credit>().Add(credit);
        await _db.SaveChangesAsync(ct);

        // Invalidate cache
        await InvalidateBalanceCacheAsync(tenantId, ct);

        _logger.LogInformation(
            "Added {Amount} credits to tenant {TenantId}. New balance: {Balance}",
            request.Amount, tenantId, newBalance);

        return Result<CreditDto>.Success(MapToDto(credit));
    }

    public async Task<Result<CreditDto>> SpendCreditsAsync(Guid tenantId, SpendCreditsRequest request, CancellationToken ct = default)
    {
        if (request.Amount <= 0)
            return Result<CreditDto>.ValidationError("Amount must be positive");

        var currentBalance = await GetCurrentBalanceAsync(tenantId, ct);

        if (currentBalance < request.Amount)
            return Result<CreditDto>.ValidationError($"Insufficient credits. Available: {currentBalance}, Required: {request.Amount}");

        var newBalance = currentBalance - request.Amount;

        var credit = new Credit
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Type = "spend",
            Amount = -request.Amount, // Negative for spending
            Balance = newBalance,
            Description = request.Description,
            ReferenceId = request.ReferenceId,
            ReferenceType = request.ReferenceType,
            UserId = _currentUser.UserId
        };

        _db.Set<Credit>().Add(credit);
        await _db.SaveChangesAsync(ct);

        // Invalidate cache
        await InvalidateBalanceCacheAsync(tenantId, ct);

        _logger.LogInformation(
            "Spent {Amount} credits from tenant {TenantId}. New balance: {Balance}",
            request.Amount, tenantId, newBalance);

        return Result<CreditDto>.Success(MapToDto(credit));
    }

    public async Task<bool> HasSufficientCreditsAsync(Guid tenantId, long amount, CancellationToken ct = default)
    {
        var balance = await GetBalanceAsync(tenantId, ct);
        return balance.Balance >= amount;
    }

    private async Task<long> GetCurrentBalanceAsync(Guid tenantId, CancellationToken ct)
    {
        var latestEntry = await _db.Set<Credit>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);

        return latestEntry?.Balance ?? 0;
    }

    private async Task InvalidateBalanceCacheAsync(Guid tenantId, CancellationToken ct)
    {
        var cacheKey = $"{BalanceCacheKeyPrefix}:{tenantId}";
        await _cache.RemoveAsync(cacheKey, ct);
    }

    private static CreditDto MapToDto(Credit c) => new()
    {
        Id = c.Id,
        TenantId = c.TenantId,
        Type = c.Type,
        Amount = c.Amount,
        Balance = c.Balance,
        Description = c.Description,
        ReferenceId = c.ReferenceId,
        ReferenceType = c.ReferenceType,
        UserId = c.UserId,
        ExpiresAt = c.ExpiresAt,
        CreatedAt = c.CreatedAt
    };
}
