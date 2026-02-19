using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SaasKit.SharedKernel.Interfaces;
using StackExchange.Redis;

namespace SaasKit.Infrastructure.Caching;

/// <summary>
/// Implementation of IRedisCacheService with tenant-aware caching.
/// </summary>
public sealed class RedisCacheService : IRedisCacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _redis;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<RedisCacheService> _logger;

    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(30);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public RedisCacheService(
        IDistributedCache cache,
        IConnectionMultiplexer redis,
        ITenantContext tenantContext,
        ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _redis = redis;
        _tenantContext = tenantContext;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = BuildTenantKey(key);

        try
        {
            var data = await _cache.GetStringAsync(fullKey, cancellationToken);
            
            if (string.IsNullOrEmpty(data))
                return default;

            return JsonSerializer.Deserialize<T>(data, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get cache key: {Key}", fullKey);
            return default;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default)
    {
        var fullKey = BuildTenantKey(key);

        try
        {
            var data = JsonSerializer.Serialize(value, JsonOptions);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl ?? DefaultTtl
            };

            await _cache.SetStringAsync(fullKey, data, options, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to set cache key: {Key}", fullKey);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = BuildTenantKey(key);

        try
        {
            await _cache.RemoveAsync(fullKey, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove cache key: {Key}", fullKey);
        }
    }

    public async Task<T?> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        
        if (cached is not null)
            return cached;

        var value = await factory(cancellationToken);
        
        if (value is not null)
        {
            await SetAsync(key, value, ttl, cancellationToken);
        }

        return value;
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        var fullPattern = BuildTenantKey(pattern);

        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: $"saaskit:{fullPattern}");

            var db = _redis.GetDatabase();
            foreach (var key in keys)
            {
                await db.KeyDeleteAsync(key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to remove cache keys by pattern: {Pattern}", fullPattern);
        }
    }

    public string BuildKey(string module, string entity, string id)
    {
        return $"{module}:{entity}:{id}";
    }

    public string BuildTenantKey(string key)
    {
        if (!_tenantContext.IsResolved)
            return $"global:{key}";

        return $"{_tenantContext.TenantId}:{key}";
    }
}
