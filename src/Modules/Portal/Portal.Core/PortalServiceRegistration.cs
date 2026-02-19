using Microsoft.Extensions.DependencyInjection;
using Portal.Contracts;
using Portal.Core.Services;
using SaasKit.Infrastructure.Tenancy;

namespace Portal.Core;

/// <summary>
/// Service registration for the Portal module.
/// </summary>
public static class PortalServiceRegistration
{
    /// <summary>
    /// Registers Portal module services.
    /// </summary>
    public static IServiceCollection AddPortalModule(this IServiceCollection services)
    {
        // Register portal context (scoped per request)
        services.AddScoped<PortalContext>();
        services.AddScoped<IPortalContext>(sp => sp.GetRequiredService<PortalContext>());

        // Register services
        services.AddScoped<IPortalService, PortalService>();
        services.AddScoped<IPortalUserService, PortalUserService>();

        return services;
    }
}
