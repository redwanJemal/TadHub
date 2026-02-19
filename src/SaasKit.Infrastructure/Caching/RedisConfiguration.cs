using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace SaasKit.Infrastructure.Caching;

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

        // Register IConnectionMultiplexer as singleton for direct Redis access
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var options = ConfigurationOptions.Parse(connectionString);
            options.AbortOnConnectFail = false;
            options.ConnectRetry = 3;
            options.ConnectTimeout = 5000;
            return ConnectionMultiplexer.Connect(options);
        });

        // Register IDistributedCache for standard caching operations
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
            options.InstanceName = "saaskit:";
        });

        // Register our custom cache service
        services.AddScoped<IRedisCacheService, RedisCacheService>();

        return services;
    }
}
