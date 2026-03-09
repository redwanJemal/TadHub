using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SupplierPortal.Contracts;
using SupplierPortal.Core.Services;

namespace SupplierPortal.Core;

public static class SupplierPortalServiceRegistration
{
    public static IServiceCollection AddSupplierPortalModule(this IServiceCollection services)
    {
        services.AddScoped<ISupplierPortalService, SupplierPortalService>();
        services.AddValidatorsFromAssembly(typeof(SupplierPortalServiceRegistration).Assembly);
        return services;
    }
}
