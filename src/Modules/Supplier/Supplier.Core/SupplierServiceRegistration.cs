using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Supplier.Contracts;
using Supplier.Core.Services;

namespace Supplier.Core;

/// <summary>
/// Supplier module service registration.
/// </summary>
public static class SupplierServiceRegistration
{
    /// <summary>
    /// Adds Supplier module services to the service collection.
    /// </summary>
    public static IServiceCollection AddSupplierModule(this IServiceCollection services)
    {
        services.AddScoped<ISupplierService, SupplierService>();
        services.AddValidatorsFromAssembly(typeof(SupplierServiceRegistration).Assembly);
        return services;
    }
}
