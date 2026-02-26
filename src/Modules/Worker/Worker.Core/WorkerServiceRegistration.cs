using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Worker.Contracts;
using Worker.Core.Services;

namespace Worker.Core;

/// <summary>
/// Worker module service registration.
/// </summary>
public static class WorkerServiceRegistration
{
    public static IServiceCollection AddWorkerModule(this IServiceCollection services)
    {
        services.AddScoped<IWorkerService, WorkerService>();
        services.AddValidatorsFromAssembly(typeof(WorkerServiceRegistration).Assembly);
        return services;
    }
}
