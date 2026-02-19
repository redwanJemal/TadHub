using Microsoft.Extensions.DependencyInjection;

namespace SaasKit.Infrastructure.Sse;

/// <summary>
/// SSE service registration.
/// </summary>
public static class SseConfiguration
{
    /// <summary>
    /// Adds SSE infrastructure services to the service collection.
    /// Requires Redis to be configured first (AddRedisCache).
    /// </summary>
    public static IServiceCollection AddSseInfrastructure(this IServiceCollection services)
    {
        // Connection manager is singleton to track all connections
        services.AddSingleton<ISseConnectionManager, SseConnectionManager>();
        
        // Notifier is singleton to maintain Redis subscriptions
        services.AddSingleton<ISseNotifier, SseNotifier>();

        return services;
    }
}
