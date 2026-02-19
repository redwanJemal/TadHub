using Analytics.Contracts;
using Analytics.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Analytics.Core;

public static class AnalyticsServiceRegistration
{
    public static IServiceCollection AddAnalyticsModule(this IServiceCollection services)
    {
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        return services;
    }
}
