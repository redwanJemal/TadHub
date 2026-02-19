using Content.Contracts;
using Content.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Content.Core;

public static class ContentServiceRegistration
{
    public static IServiceCollection AddContentModule(this IServiceCollection services)
    {
        services.AddScoped<IBlogService, BlogService>();
        return services;
    }
}
