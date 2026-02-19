using Authorization.Contracts;
using Authorization.Core.Seeds;
using Authorization.Core.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using SaasKit.Infrastructure.Auth;
using SaasKit.SharedKernel.Interfaces;

namespace Authorization.Core;

/// <summary>
/// Authorization module service registration.
/// </summary>
public static class AuthorizationServiceRegistration
{
    /// <summary>
    /// Adds Authorization module services to the service collection.
    /// </summary>
    public static IServiceCollection AddAuthorizationModule(this IServiceCollection services)
    {
        // Core services
        services.AddScoped<IAuthorizationModuleService, AuthorizationModuleService>();
        
        // Permission checker (for policy handler)
        services.AddScoped<IPermissionChecker, PermissionCheckerAdapter>();

        // Permission seeder (runs on startup)
        services.AddHostedService<PermissionSeeder>();

        // Authorization policy provider
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(typeof(AuthorizationServiceRegistration).Assembly);

        return services;
    }
}

/// <summary>
/// Adapter that implements IPermissionChecker using IAuthorizationModuleService.
/// </summary>
internal class PermissionCheckerAdapter : IPermissionChecker
{
    private readonly IAuthorizationModuleService _authService;

    public PermissionCheckerAdapter(IAuthorizationModuleService authService)
    {
        _authService = authService;
    }

    public Task<bool> HasPermissionAsync(Guid tenantId, Guid userId, string permission, CancellationToken ct = default)
    {
        return _authService.HasPermissionAsync(tenantId, userId, permission, ct);
    }
}
