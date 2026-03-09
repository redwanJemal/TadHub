using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Placement.Contracts;
using Placement.Core.Services;

namespace Placement.Core;

public static class PlacementServiceRegistration
{
    public static IServiceCollection AddPlacementModule(this IServiceCollection services)
    {
        services.AddScoped<IPlacementService, PlacementService>();
        services.AddValidatorsFromAssembly(typeof(PlacementServiceRegistration).Assembly);
        return services;
    }
}
