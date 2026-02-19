using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TadHub.Infrastructure.Settings;

namespace TadHub.Infrastructure.Jobs;

/// <summary>
/// Hangfire background job configuration.
/// </summary>
public static class HangfireConfiguration
{
    /// <summary>
    /// Adds Hangfire services with PostgreSQL storage.
    /// </summary>
    public static IServiceCollection AddBackgroundJobs(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration.GetSection(HangfireSettings.SectionName).Get<HangfireSettings>()
            ?? new HangfireSettings();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.Configure<HangfireSettings>(configuration.GetSection(HangfireSettings.SectionName));

        services.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options =>
                {
                    options.UseNpgsqlConnection(connectionString);
                }, new PostgreSqlStorageOptions
                {
                    SchemaName = "hangfire",
                    PrepareSchemaIfNecessary = true,
                    QueuePollInterval = TimeSpan.FromSeconds(15),
                    InvisibilityTimeout = TimeSpan.FromMinutes(30),
                    DistributedLockTimeout = TimeSpan.FromMinutes(10)
                });
        });

        // Add Hangfire server
        services.AddHangfireServer(options =>
        {
            options.WorkerCount = settings.WorkerCount;
            options.Queues = settings.Queues;
            options.ServerName = $"{Environment.MachineName}:{Guid.NewGuid().ToString("N")[..8]}";
        });

        return services;
    }

    /// <summary>
    /// Configures Hangfire dashboard endpoint.
    /// Only accessible to platform-admin role.
    /// </summary>
    public static IApplicationBuilder UseHangfireDashboard(
        this IApplicationBuilder app,
        IConfiguration configuration)
    {
        var settings = configuration.GetSection(HangfireSettings.SectionName).Get<HangfireSettings>()
            ?? new HangfireSettings();

        app.UseHangfireDashboard(settings.DashboardPath, new DashboardOptions
        {
            Authorization = [new HangfireDashboardAuthorizationFilter()],
            DashboardTitle = "SaaS Kit Jobs",
            DisplayStorageConnectionString = false,
            IsReadOnlyFunc = context => !context.GetHttpContext().User.IsInRole("platform-admin")
        });

        return app;
    }
}

/// <summary>
/// Dashboard authorization filter - requires platform-admin role.
/// </summary>
public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        
        // In development, allow access
        var env = httpContext.RequestServices
            .GetService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
        
        if (env?.EnvironmentName == "Development")
            return true;

        // In production, require platform-admin role
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole("platform-admin");
    }
}
