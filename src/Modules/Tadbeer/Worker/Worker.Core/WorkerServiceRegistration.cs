using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Worker.Contracts;
using Worker.Core.Services;
using Worker.Core.StateMachine;

namespace Worker.Core;

/// <summary>
/// Worker module service registration.
/// </summary>
public static class WorkerServiceRegistration
{
    /// <summary>
    /// Adds Worker module services to the service collection.
    /// </summary>
    public static IServiceCollection AddWorkerModule(this IServiceCollection services)
    {
        // State machine
        services.AddSingleton<StateTransitionValidator>();

        // Services
        services.AddScoped<IWorkerService, WorkerService>();

        // FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(typeof(WorkerServiceRegistration).Assembly);

        return services;
    }
}
