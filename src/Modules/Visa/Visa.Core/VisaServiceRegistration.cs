using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Visa.Contracts;
using Visa.Core.Services;

namespace Visa.Core;

public static class VisaServiceRegistration
{
    public static IServiceCollection AddVisaModule(this IServiceCollection services)
    {
        services.AddScoped<IVisaApplicationService, VisaApplicationService>();
        services.AddValidatorsFromAssembly(typeof(VisaServiceRegistration).Assembly);
        return services;
    }
}
