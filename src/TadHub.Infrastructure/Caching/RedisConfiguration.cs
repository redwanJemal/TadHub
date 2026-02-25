using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace TadHub.Infrastructure.Caching;

/// <summary>
/// Redis configuration and service registration.
/// </summary>
public static class RedisConfiguration
{
    /// <summary>
    /// Adds Redis caching services to the service collection.
    /// </summary>
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Redis")
            ?? "localhost:6379";

        // Ensure the connection string has AbortOnConnectFail=false
        var fullConnectionString = connectionString.Contains("abortConnect")
            ? connectionString
            : connectionString + ",abortConnect=false,connectRetry=5,connectTimeout=10000";

        // Register IConnectionMultiplexer as singleton for direct Redis access
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var options = ConfigurationOptions.Parse(fullConnectionString);
            return ConnectionMultiplexer.Connect(options);
        });

        // Register IDistributedCache for standard caching operations
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = fullConnectionString;
            options.InstanceName = "tadhub:";
        });

        // Register our custom cache service
        services.AddScoped<IRedisCacheService, RedisCacheService>();

        return services;
    }
}
