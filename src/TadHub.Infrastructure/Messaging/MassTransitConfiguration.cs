using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TadHub.Infrastructure.Messaging.Filters;
using TadHub.Infrastructure.Settings;

namespace TadHub.Infrastructure.Messaging;

/// <summary>
/// MassTransit configuration for RabbitMQ messaging.
/// </summary>
public static class MassTransitConfiguration
{
    /// <summary>
    /// Adds MassTransit with RabbitMQ transport to the service collection.
    /// </summary>
    public static IServiceCollection AddMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] consumerAssemblies)
    {
        var settings = configuration.GetSection(RabbitMqSettings.SectionName).Get<RabbitMqSettings>()
            ?? new RabbitMqSettings();

        services.AddMassTransit(busConfig =>
        {
            // Register consumers from provided assemblies
            foreach (var assembly in consumerAssemblies)
            {
                busConfig.AddConsumers(assembly);
            }

            // Also scan module assemblies for consumers
            var moduleAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetName().Name?.EndsWith(".Core") == true);

            foreach (var assembly in moduleAssemblies)
            {
                busConfig.AddConsumers(assembly);
            }

            // Configure RabbitMQ transport
            busConfig.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(settings.Host, (ushort)settings.Port, settings.VirtualHost, h =>
                {
                    h.Username(settings.Username);
                    h.Password(settings.Password);
                });

                // Configure retry policy: 3 immediate retries, then move to _error queue
                cfg.UseMessageRetry(r => r.Immediate(3));

                // MassTransit 8.x uses System.Text.Json by default
                // Configure JSON serialization options
                cfg.ConfigureJsonSerializerOptions(opts =>
                {
                    opts.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    return opts;
                });

                // Add tenant context to outgoing messages
                cfg.UsePublishFilter(typeof(TenantContextPublishFilter<>), context);
                cfg.UseSendFilter(typeof(TenantContextSendFilter<>), context);

                // Configure endpoints from registered consumers
                cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter("saaskit", false));
            });
        });

        return services;
    }

    /// <summary>
    /// Adds MassTransit test harness for in-memory testing.
    /// </summary>
    public static IServiceCollection AddMessagingTestHarness(
        this IServiceCollection services,
        params Assembly[] consumerAssemblies)
    {
        services.AddMassTransitTestHarness(busConfig =>
        {
            // Register consumers from provided assemblies
            foreach (var assembly in consumerAssemblies)
            {
                busConfig.AddConsumers(assembly);
            }

            // Use in-memory transport for testing
            busConfig.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context, new KebabCaseEndpointNameFormatter("saaskit", false));
            });
        });

        return services;
    }
}
