using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Arrival.Contracts;
using Arrival.Core.Services;

namespace Arrival.Core;

public static class ArrivalServiceRegistration
{
    public static IServiceCollection AddArrivalModule(this IServiceCollection services)
    {
        services.AddScoped<IArrivalService, ArrivalService>();
        services.AddValidatorsFromAssembly(typeof(ArrivalServiceRegistration).Assembly);
        return services;
    }
}
