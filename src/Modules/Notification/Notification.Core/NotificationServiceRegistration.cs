using Microsoft.Extensions.DependencyInjection;
using Notification.Contracts;
using Notification.Core.Services;

namespace Notification.Core;

/// <summary>
/// Service registration for the Notification module.
/// </summary>
public static class NotificationServiceRegistration
{
    /// <summary>
    /// Registers Notification module services.
    /// </summary>
    public static IServiceCollection AddNotificationModule(this IServiceCollection services)
    {
        // Register the notification service
        services.AddScoped<INotificationModuleService, NotificationModuleService>();

        return services;
    }
}
