using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using SaasKit.SharedKernel.Interfaces;

namespace SaasKit.Infrastructure.Auth;

/// <summary>
/// Authorization handler that checks for specific permissions.
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionChecker _permissionChecker;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(
        IPermissionChecker permissionChecker,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        ILogger<PermissionAuthorizationHandler> logger)
    {
        _permissionChecker = permissionChecker;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (!_currentUser.IsAuthenticated)
        {
            _logger.LogDebug("Permission check failed: user not authenticated");
            return;
        }

        if (!_tenantContext.IsResolved)
        {
            _logger.LogDebug("Permission check failed: no tenant context");
            return;
        }

        var hasPermission = await _permissionChecker.HasPermissionAsync(
            _tenantContext.TenantId,
            _currentUser.UserId,
            requirement.Permission);

        if (hasPermission)
        {
            _logger.LogDebug(
                "Permission check passed: user {UserId} has {Permission} in tenant {TenantId}",
                _currentUser.UserId, requirement.Permission, _tenantContext.TenantId);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogDebug(
                "Permission check failed: user {UserId} lacks {Permission} in tenant {TenantId}",
                _currentUser.UserId, requirement.Permission, _tenantContext.TenantId);
        }
    }
}

/// <summary>
/// Policy provider that creates permission-based policies dynamically.
/// </summary>
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private const string PolicyPrefix = "Permission:";
    private readonly DefaultAuthorizationPolicyProvider _fallbackProvider;

    public PermissionPolicyProvider(Microsoft.Extensions.Options.IOptions<AuthorizationOptions> options)
    {
        _fallbackProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(PolicyPrefix))
        {
            var permission = policyName[PolicyPrefix.Length..];
            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallbackProvider.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallbackProvider.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackProvider.GetFallbackPolicyAsync();
    }
}
