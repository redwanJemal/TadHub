using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Accommodation.Contracts;
using Accommodation.Core.Services;

namespace Accommodation.Core;

public static class AccommodationServiceRegistration
{
    public static IServiceCollection AddAccommodationModule(this IServiceCollection services)
    {
        services.AddScoped<IAccommodationService, AccommodationService>();
        services.AddValidatorsFromAssembly(typeof(AccommodationServiceRegistration).Assembly);
        return services;
    }
}
