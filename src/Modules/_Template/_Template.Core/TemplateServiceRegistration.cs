using _Template.Contracts;
using _Template.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace _Template.Core;

public static class TemplateServiceRegistration
{
    public static IServiceCollection AddTemplateModule(this IServiceCollection services)
    {
        services.AddScoped<ITemplateService, TemplateService>();
        return services;
    }
}
