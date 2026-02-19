using ApiManagement.Contracts;
using ApiManagement.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ApiManagement.Core;

public static class ApiManagementServiceRegistration
{
    public static IServiceCollection AddApiManagementModule(this IServiceCollection services)
    {
        services.AddScoped<IApiKeyService, ApiKeyService>();
        return services;
    }
}
