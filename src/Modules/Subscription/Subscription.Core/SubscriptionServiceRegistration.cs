using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SaasKit.Infrastructure.Settings;
using Subscription.Contracts;
using Subscription.Core.Gateways;
using Subscription.Core.Seeds;
using Subscription.Core.Services;

namespace Subscription.Core;

/// <summary>
/// Service registration for the Subscription module.
/// </summary>
public static class SubscriptionServiceRegistration
{
    /// <summary>
    /// Registers Subscription module services.
    /// </summary>
    public static IServiceCollection AddSubscriptionModule(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Stripe settings
        services.Configure<StripeSettings>(configuration.GetSection(StripeSettings.SectionName));

        // Register services
        services.AddScoped<IPlanService, PlanService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IFeatureGateService, FeatureGateService>();
        services.AddScoped<ICreditService, CreditService>();

        // Register Stripe gateway
        services.AddScoped<IPaymentGateway, StripePaymentGateway>();

        // Register plan seeder
        services.AddHostedService<PlanSeeder>();

        return services;
    }
}
