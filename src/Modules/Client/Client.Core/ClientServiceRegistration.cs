using Client.Contracts;
using Client.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Client.Core;

public static class ClientServiceRegistration
{
    public static IServiceCollection AddClientModule(this IServiceCollection services)
    {
        services.AddScoped<IClientService, ClientService>();
        return services;
    }
}
