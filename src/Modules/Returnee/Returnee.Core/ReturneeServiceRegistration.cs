using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Returnee.Contracts;
using Returnee.Core.Services;

namespace Returnee.Core;

public static class ReturneeServiceRegistration
{
    public static IServiceCollection AddReturneeModule(this IServiceCollection services)
    {
        services.AddScoped<IReturneeService, ReturneeService>();
        services.AddValidatorsFromAssembly(typeof(ReturneeServiceRegistration).Assembly);
        return services;
    }
}
