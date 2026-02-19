namespace SaasKit.Infrastructure.Caching;

/// <summary>
/// High-level Redis cache service with tenant-aware key management.
/// Key convention: saaskit:{tenant_id}:{module}:{entity}:{id}
/// </summary>
public interface IRedisCacheService
{
    /// <summary>
    /// Gets a value from the cache.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key (will be prefixed with tenant context).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached value, or default if not found.</returns>
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a value in the cache with optional TTL.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="key">The cache key (will be prefixed with tenant context).</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="ttl">Time to live. If null, uses default expiration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a value from the cache.
    /// </summary>
    /// <param name="key">The cache key (will be prefixed with tenant context).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a value from cache, or creates and caches it if not found.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key (will be prefixed with tenant context).</param>
    /// <param name="factory">Factory function to create the value if not cached.</param>
    /// <param name="ttl">Time to live. If null, uses default expiration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The cached or newly created value.</returns>
    Task<T?> GetOrSetAsync<T>(
        string key,
        Func<CancellationToken, Task<T>> factory,
        TimeSpan? ttl = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all cache entries matching a pattern.
    /// </summary>
    /// <param name="pattern">The pattern to match (e.g., "users:*").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds a cache key with the standard convention.
    /// </summary>
    /// <param name="module">The module name (e.g., "identity", "tenancy").</param>
    /// <param name="entity">The entity type (e.g., "user", "tenant").</param>
    /// <param name="id">The entity identifier.</param>
    /// <returns>The formatted cache key.</returns>
    string BuildKey(string module, string entity, string id);

    /// <summary>
    /// Builds a cache key with tenant prefix.
    /// </summary>
    /// <param name="key">The base key.</param>
    /// <returns>The key prefixed with tenant context.</returns>
    string BuildTenantKey(string key);
}
