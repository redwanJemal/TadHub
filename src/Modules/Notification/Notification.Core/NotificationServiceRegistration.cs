using Microsoft.Extensions.DependencyInjection;
using Notification.Contracts;
using Notification.Contracts.Channels;
using Notification.Core.Channels;
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
        // Core notification service
        services.AddScoped<INotificationModuleService, NotificationModuleService>();

        // Notification channels
        services.AddScoped<INotificationChannel, InAppNotificationChannel>();
        services.AddScoped<INotificationChannel, EmailNotificationChannel>();

        // Dispatcher
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();

        // Settings & recipients
        services.AddScoped<ITenantNotificationSettingsProvider, TenantNotificationSettingsProvider>();
        services.AddScoped<INotificationRecipientResolver, NotificationRecipientResolver>();

        return services;
    }
}
