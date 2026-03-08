using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Runaway.Contracts;
using Runaway.Core.Services;

namespace Runaway.Core;

public static class RunawayServiceRegistration
{
    public static IServiceCollection AddRunawayModule(this IServiceCollection services)
    {
        services.AddScoped<IRunawayService, RunawayService>();
        services.AddValidatorsFromAssembly(typeof(RunawayServiceRegistration).Assembly);
        return services;
    }
}
