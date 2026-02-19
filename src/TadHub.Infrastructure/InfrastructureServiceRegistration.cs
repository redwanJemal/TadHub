using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Auth;
using TadHub.Infrastructure.Caching;
using TadHub.Infrastructure.Clock;
using TadHub.Infrastructure.Jobs;
using TadHub.Infrastructure.Keycloak;
using TadHub.Infrastructure.Messaging;
using TadHub.Infrastructure.Persistence;
using TadHub.Infrastructure.Persistence.Interceptors;
using TadHub.Infrastructure.Search;
using TadHub.Infrastructure.Settings;
using TadHub.Infrastructure.Sse;
using TadHub.Infrastructure.Storage;
using TadHub.SharedKernel.Interfaces;

namespace TadHub.Infrastructure;

/// <summary>
/// Infrastructure layer service registration.
/// Consolidates all infrastructure services into a single extension method.
/// </summary>
public static class InfrastructureServiceRegistration
{
    /// <summary>
    /// Adds all infrastructure services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="consumerAssemblies">Optional assemblies to scan for MassTransit consumers.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] consumerAssemblies)
    {
        // =============================================================================
        // Core Services
        // =============================================================================
        
        // Clock service (for testable time)
        services.AddSingleton<IClock, SystemClock>();

        // HTTP context accessor (required for CurrentUser and TenantContext)
        services.AddHttpContextAccessor();

        // Tenant context (from HTTP headers, JWT claims, or query params)
        services.AddScoped<TenantContext>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

        // =============================================================================
        // API Infrastructure
        // =============================================================================
        
        // Model binder for QueryParameters
        services.Configure<MvcOptions>(options =>
        {
            options.ModelBinderProviders.Insert(0, new QueryParametersModelBinderProvider());
        });

        // Global exception handler (.NET 9 IExceptionHandler)
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // FluentValidation filter
        services.AddScoped<FluentValidationFilter>();
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<FluentValidationFilter>();
        });

        // =============================================================================
        // Persistence (EF Core + PostgreSQL)
        // =============================================================================
        
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection connection string not configured");

        // Register interceptors
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<TenantIdInterceptor>();
        services.AddScoped<SoftDeleteInterceptor>();
        services.AddScoped<RlsInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
                
                npgsql.CommandTimeout(30);
                npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });

            // Add interceptors
            options.AddInterceptors(
                sp.GetRequiredService<AuditableEntityInterceptor>(),
                sp.GetRequiredService<TenantIdInterceptor>(),
                sp.GetRequiredService<SoftDeleteInterceptor>(),
                sp.GetRequiredService<RlsInterceptor>()
            );

            // Use snake_case naming convention
            options.UseSnakeCaseNamingConvention();
        });

        // Register IUnitOfWork interface pointing to AppDbContext
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        // =============================================================================
        // Caching (Redis)
        // =============================================================================
        
        services.AddRedisCache(configuration);

        // =============================================================================
        // Messaging (MassTransit + RabbitMQ)
        // =============================================================================
        
        services.AddMessaging(configuration, consumerAssemblies);

        // =============================================================================
        // SSE (Server-Sent Events)
        // =============================================================================
        
        services.AddSseInfrastructure();

        // =============================================================================
        // File Storage (MinIO)
        // =============================================================================
        
        services.AddFileStorage(configuration);

        // =============================================================================
        // Search (Meilisearch)
        // =============================================================================
        
        services.AddSearch(configuration);

        // =============================================================================
        // Background Jobs (Hangfire)
        // =============================================================================
        
        services.AddBackgroundJobs(configuration);

        // =============================================================================
        // Keycloak Admin API
        // =============================================================================
        
        services.AddKeycloakAdmin(configuration);

        // =============================================================================
        // Health Checks
        // =============================================================================
        
        var featureSettings = configuration.GetSection(FeatureSettings.SectionName).Get<FeatureSettings>()
            ?? new FeatureSettings();

        if (featureSettings.EnableHealthChecks)
        {
            services.AddHealthChecks()
                .AddNpgSql(connectionString, name: "postgresql")
                .AddRedis(
                    configuration.GetConnectionString("Redis") ?? "localhost:6379",
                    name: "redis")
                .AddRabbitMQ(sp =>
                    {
                        var settings = configuration.GetSection(RabbitMqSettings.SectionName).Get<RabbitMqSettings>()
                            ?? new RabbitMqSettings();
                        
                        var factory = new RabbitMQ.Client.ConnectionFactory
                        {
                            HostName = settings.Host,
                            Port = settings.Port,
                            UserName = settings.Username,
                            Password = settings.Password,
                            VirtualHost = settings.VirtualHost
                        };
                        return factory.CreateConnectionAsync().GetAwaiter().GetResult();
                    },
                    name: "rabbitmq");
        }

        return services;
    }

    /// <summary>
    /// Configures the infrastructure middleware pipeline.
    /// Call this in the order: UseInfrastructure() -> UseRouting() -> ...
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseInfrastructure(
        this IApplicationBuilder app,
        IConfiguration configuration)
    {
        // Global exception handler
        app.UseExceptionHandler();

        // Response envelope wrapping middleware
        app.UseMiddleware<ApiResponseWrappingMiddleware>();

        return app;
    }

    /// <summary>
    /// Maps infrastructure endpoints (health, hangfire dashboard).
    /// Call after MapControllers().
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication MapInfrastructureEndpoints(
        this WebApplication app,
        IConfiguration configuration)
    {
        var featureSettings = configuration.GetSection(FeatureSettings.SectionName).Get<FeatureSettings>()
            ?? new FeatureSettings();

        // Health check endpoint
        if (featureSettings.EnableHealthChecks)
        {
            app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = WriteHealthCheckResponse
            });

            app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("live"),
                ResponseWriter = WriteHealthCheckResponse
            });
        }

        // Hangfire dashboard (requires platform-admin role)
        if (featureSettings.EnableHangfireDashboard)
        {
            app.UseHangfireDashboard(configuration);
        }

        return app;
    }

    private static string BuildRabbitMqConnectionString(IConfiguration configuration)
    {
        var settings = configuration.GetSection(RabbitMqSettings.SectionName).Get<RabbitMqSettings>()
            ?? new RabbitMqSettings();

        return $"amqp://{settings.Username}:{settings.Password}@{settings.Host}:{settings.Port}{settings.VirtualHost}";
    }

    private static async Task WriteHealthCheckResponse(
        Microsoft.AspNetCore.Http.HttpContext context,
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds,
                description = e.Value.Description,
                exception = e.Value.Exception?.Message
            })
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}
