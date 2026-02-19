using Audit.Contracts;
using Audit.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Audit.Core;

public static class AuditServiceRegistration
{
    public static IServiceCollection AddAuditModule(this IServiceCollection services)
    {
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IWebhookService, WebhookService>();
        return services;
    }
}
