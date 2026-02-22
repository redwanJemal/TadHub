using FluentValidation;
using Identity.Contracts;
using Identity.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using TadHub.Infrastructure.Auth;
using TadHub.SharedKernel.Interfaces;

namespace Identity.Core;

/// <summary>
/// Identity module service registration.
/// </summary>
public static class IdentityServiceRegistration
{
    /// <summary>
    /// Adds Identity module services to the service collection.
    /// </summary>
    public static IServiceCollection AddIdentityModule(this IServiceCollection services)
    {
        // Core services
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IPlatformStaffService, PlatformStaffService>();

        // Current user (from JWT claims)
        services.AddHttpContextAccessor();
        services.AddScoped<CurrentUser>();
        services.AddScoped<ICurrentUser>(sp => sp.GetRequiredService<CurrentUser>());

        // FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(typeof(IdentityServiceRegistration).Assembly);

        return services;
    }
}
