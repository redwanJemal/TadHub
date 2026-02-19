using FeatureFlags.Contracts;
using FeatureFlags.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FeatureFlags.Core;

public static class FeatureFlagsServiceRegistration
{
    public static IServiceCollection AddFeatureFlagsModule(this IServiceCollection services)
    {
        services.AddScoped<IFeatureFlagService, FeatureFlagService>();
        return services;
    }
}
