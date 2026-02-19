using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Tenancy.Contracts;
using Tenancy.Core.Services;

namespace Tenancy.Core;

/// <summary>
/// Tenancy module service registration.
/// </summary>
public static class TenancyServiceRegistration
{
    /// <summary>
    /// Adds Tenancy module services to the service collection.
    /// </summary>
    public static IServiceCollection AddTenancyModule(this IServiceCollection services)
    {
        // Core services
        services.AddScoped<ITenantService, TenantService>();

        // FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(typeof(TenancyServiceRegistration).Assembly);

        return services;
    }
}
