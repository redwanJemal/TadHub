using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text.Json;
using ApiManagement.Contracts;
using ApiManagement.Contracts.DTOs;
using ApiManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Caching;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;

namespace ApiManagement.Core.Services;

public class ApiKeyService : IApiKeyService
{
    private readonly AppDbContext _db;
    private readonly IRedisCacheService _cache;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<ApiKeyService> _logger;

    private const string ApiKeyCachePrefix = "apikey";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private static readonly Dictionary<string, Expression<Func<ApiKey, object>>> ApiKeyFilters = new()
    {
        ["isActive"] = x => x.IsActive,
        ["name"] = x => x.Name
    };

    private static readonly Dictionary<string, Expression<Func<ApiKeyLog, object>>> LogFilters = new()
    {
        ["statusCode"] = x => x.StatusCode,
        ["method"] = x => x.Method,
        ["createdAt"] = x => x.CreatedAt
    };

    public ApiKeyService(AppDbContext db, IRedisCacheService cache, ICurrentUser currentUser, ILogger<ApiKeyService> logger)
    {
        _db = db;
        _cache = cache;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<PagedList<ApiKeyDto>> GetApiKeysAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<ApiKey>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ApplyFilters(qp.Filters, ApiKeyFilters)
            .ApplySort(qp.GetSortFields(), new Dictionary<string, Expression<Func<ApiKey, object>>>
            {
                ["createdAt"] = x => x.CreatedAt,
                ["lastUsedAt"] = x => x.LastUsedAt!,
                ["name"] = x => x.Name
            });

        return await query.Select(x => MapToDto(x)).ToPagedListAsync(qp, ct);
    }

    public async Task<Result<ApiKeyDto>> GetApiKeyByIdAsync(Guid tenantId, Guid apiKeyId, CancellationToken ct = default)
    {
        var key = await _db.Set<ApiKey>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == apiKeyId && x.TenantId == tenantId, ct);

        if (key is null)
            return Result<ApiKeyDto>.NotFound("API key not found");

        return Result<ApiKeyDto>.Success(MapToDto(key));
    }

    public async Task<Result<ApiKeyWithSecretDto>> CreateApiKeyAsync(Guid tenantId, CreateApiKeyRequest request, CancellationToken ct = default)
    {
        var secret = GenerateApiKey();
        var hash = HashApiKey(secret);
        var prefix = secret[..8];

        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Prefix = prefix,
            KeyHash = hash,
            Permissions = request.Permissions != null ? JsonSerializer.Serialize(request.Permissions) : null,
            ExpiresAt = request.ExpiresAt,
            RateLimitPerMinute = request.RateLimitPerMinute,
            IsActive = true,
            CreatedByUserId = _currentUser.UserId
        };

        _db.Set<ApiKey>().Add(apiKey);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created API key {ApiKeyId} for tenant {TenantId}", apiKey.Id, tenantId);

        return Result<ApiKeyWithSecretDto>.Success(new ApiKeyWithSecretDto
        {
            Id = apiKey.Id,
            TenantId = apiKey.TenantId,
            Name = apiKey.Name,
            Prefix = apiKey.Prefix,
            Permissions = request.Permissions,
            ExpiresAt = apiKey.ExpiresAt,
            IsActive = apiKey.IsActive,
            RateLimitPerMinute = apiKey.RateLimitPerMinute,
            CreatedAt = apiKey.CreatedAt,
            Secret = secret
        });
    }

    public async Task<Result<bool>> RevokeApiKeyAsync(Guid tenantId, Guid apiKeyId, CancellationToken ct = default)
    {
        var key = await _db.Set<ApiKey>()
            .FirstOrDefaultAsync(x => x.Id == apiKeyId && x.TenantId == tenantId, ct);

        if (key is null)
            return Result<bool>.NotFound("API key not found");

        key.IsActive = false;
        await _db.SaveChangesAsync(ct);

        await _cache.RemoveAsync($"{ApiKeyCachePrefix}:{key.KeyHash}", ct);

        _logger.LogInformation("Revoked API key {ApiKeyId}", apiKeyId);

        return Result<bool>.Success(true);
    }

    public async Task<PagedList<ApiKeyLogDto>> GetApiKeyLogsAsync(Guid tenantId, Guid apiKeyId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<ApiKeyLog>()
            .AsNoTracking()
            .Where(x => x.ApiKeyId == apiKeyId && x.TenantId == tenantId)
            .ApplyFilters(qp.Filters, LogFilters)
            .ApplySort(qp.GetSortFields(), new Dictionary<string, Expression<Func<ApiKeyLog, object>>>
            {
                ["createdAt"] = x => x.CreatedAt,
                ["statusCode"] = x => x.StatusCode
            });

        return await query.Select(x => new ApiKeyLogDto
        {
            Id = x.Id,
            ApiKeyId = x.ApiKeyId,
            Endpoint = x.Endpoint,
            Method = x.Method,
            StatusCode = x.StatusCode,
            DurationMs = x.DurationMs,
            IpAddress = x.IpAddress,
            CreatedAt = x.CreatedAt
        }).ToPagedListAsync(qp, ct);
    }

    public async Task<Result<ApiKeyDto>> ValidateApiKeyAsync(string apiKey, CancellationToken ct = default)
    {
        var hash = HashApiKey(apiKey);
        var cacheKey = $"{ApiKeyCachePrefix}:{hash}";

        var cached = await _cache.GetAsync<ApiKeyDto>(cacheKey, ct);
        if (cached != null)
            return Result<ApiKeyDto>.Success(cached);

        var key = await _db.Set<ApiKey>()
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.KeyHash == hash, ct);

        if (key is null)
            return Result<ApiKeyDto>.NotFound("Invalid API key");

        if (!key.IsActive)
            return Result<ApiKeyDto>.ValidationError("API key is revoked");

        if (key.ExpiresAt.HasValue && key.ExpiresAt < DateTimeOffset.UtcNow)
            return Result<ApiKeyDto>.ValidationError("API key has expired");

        var dto = MapToDto(key);
        await _cache.SetAsync(cacheKey, dto, CacheDuration, ct);

        return Result<ApiKeyDto>.Success(dto);
    }

    public async Task RecordApiKeyUsageAsync(Guid apiKeyId, string endpoint, string method, int statusCode, int durationMs, string? ipAddress, CancellationToken ct = default)
    {
        var key = await _db.Set<ApiKey>().FindAsync([apiKeyId], ct);
        if (key is null) return;

        key.LastUsedAt = DateTimeOffset.UtcNow;
        key.RequestCount++;

        var log = new ApiKeyLog
        {
            Id = Guid.NewGuid(),
            TenantId = key.TenantId,
            ApiKeyId = apiKeyId,
            Endpoint = endpoint,
            Method = method,
            StatusCode = statusCode,
            DurationMs = durationMs,
            IpAddress = ipAddress
        };

        _db.Set<ApiKeyLog>().Add(log);
        await _db.SaveChangesAsync(ct);
    }

    private static string GenerateApiKey()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return $"sk_{Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "")}";
    }

    private static string HashApiKey(string apiKey)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(apiKey);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static ApiKeyDto MapToDto(ApiKey k) => new()
    {
        Id = k.Id,
        TenantId = k.TenantId,
        Name = k.Name,
        Prefix = k.Prefix,
        Permissions = k.Permissions != null ? JsonSerializer.Deserialize<List<string>>(k.Permissions) : null,
        ExpiresAt = k.ExpiresAt,
        IsActive = k.IsActive,
        LastUsedAt = k.LastUsedAt,
        RequestCount = k.RequestCount,
        RateLimitPerMinute = k.RateLimitPerMinute,
        CreatedAt = k.CreatedAt
    };
}
