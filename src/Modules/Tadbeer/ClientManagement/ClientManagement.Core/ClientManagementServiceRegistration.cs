using ClientManagement.Contracts;
using ClientManagement.Core.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ClientManagement.Core;

/// <summary>
/// Client Management module service registration.
/// </summary>
public static class ClientManagementServiceRegistration
{
    /// <summary>
    /// Adds Client Management module services to the service collection.
    /// </summary>
    public static IServiceCollection AddClientManagementModule(this IServiceCollection services)
    {
        // Services
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<ILeadService, LeadService>();

        // FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(typeof(ClientManagementServiceRegistration).Assembly);

        return services;
    }
}
