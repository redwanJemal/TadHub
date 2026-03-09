using Microsoft.Extensions.DependencyInjection;
using Reporting.Contracts;
using Reporting.Core.Services;

namespace Reporting.Core;

public static class ReportingServiceRegistration
{
    public static IServiceCollection AddReportingModule(this IServiceCollection services)
    {
        services.AddScoped<IReportService, ReportService>();
        return services;
    }
}
