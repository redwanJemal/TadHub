using Authorization.Contracts;
using Identity.Contracts;
using Identity.Contracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TadHub.Infrastructure.Auth;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Interfaces;
using Tenancy.Contracts;

namespace TadHub.Api.Controllers;

/// <summary>
/// Current user shortcut endpoint - aliases /users/me for convenience.
/// </summary>
[ApiController]
[Route("api/v1/me")]
[Authorize]
public class MeController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly ITenantService _tenantService;
    private readonly IAuthorizationModuleService _authorizationService;
    private readonly ITenantContext _tenantContext;
    private readonly CurrentUser _currentUser;

    public MeController(
        IIdentityService identityService,
        ITenantService tenantService,
        IAuthorizationModuleService authorizationService,
        ITenantContext tenantContext,
        CurrentUser currentUser)
    {
        _identityService = identityService;
        _tenantService = tenantService;
        _authorizationService = authorizationService;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Gets the current authenticated user's profile with onboarding status.
    /// Creates the profile if it doesn't exist (JIT provisioning).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(UserOnboardingStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        // JIT provisioning: get or create user profile from JWT claims
        var userResult = await _identityService.GetOrCreateFromKeycloakAsync(
            _currentUser.KeycloakId,
            _currentUser.Email,
            _currentUser.FirstName,
            _currentUser.LastName,
            ct);

        if (!userResult.IsSuccess)
            return NotFound(new { error = userResult.Error });

        var user = userResult.Value!;

        // Get user's tenants - wrapped in try/catch for now
        List<TenantSummaryDto> tenants = [];
        try
        {
            var tenantsList = await _tenantService.ListUserTenantsAsync(user.Id, new QueryParameters { PageSize = 100 }, ct);
            tenants = tenantsList.Items.Select(t => new TenantSummaryDto
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug
            }).ToList();
        }
        catch
        {
            // Tenant service not yet available, continue without tenants
        }

        // Determine onboarding status
        var needsOnboarding = !tenants.Any();
        var needsTenantSelection = tenants.Count > 1 && user.DefaultTenantId == null;

        // Fetch permissions if tenant is resolved and user has tenants
        List<string> permissions = [];
        List<string> roles = [];

        // Determine the effective tenant for permission lookup:
        // 1. Explicitly resolved (X-Tenant-ID header / subdomain)
        // 2. User's default tenant
        // 3. User's only tenant (single-tenant shortcut)
        Guid? effectiveTenantId = null;
        if (_tenantContext.IsResolved)
        {
            effectiveTenantId = _tenantContext.TenantId;
        }
        else if (user.DefaultTenantId.HasValue && tenants.Any(t => t.Id == user.DefaultTenantId.Value))
        {
            effectiveTenantId = user.DefaultTenantId.Value;
        }
        else if (tenants.Count == 1)
        {
            effectiveTenantId = tenants[0].Id;
        }

        if (effectiveTenantId.HasValue && tenants.Count > 0)
        {
            try
            {
                var userPerms = await _authorizationService.GetUserPermissionsAsync(
                    effectiveTenantId.Value, user.Id, ct);
                permissions = userPerms.Permissions.ToList();
                roles = userPerms.Roles.ToList();
            }
            catch
            {
                // Authorization service not available, continue without permissions
            }
        }

        return Ok(new UserOnboardingStatusDto
        {
            Id = user.Id,
            KeycloakId = user.KeycloakId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = user.FullName,
            Locale = user.Locale,
            IsActive = user.IsActive,
            DefaultTenantId = user.DefaultTenantId,
            NeedsOnboarding = needsOnboarding,
            NeedsTenantSelection = needsTenantSelection,
            Tenants = tenants,
            Permissions = permissions,
            Roles = roles,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        });
    }
}

/// <summary>
/// User onboarding status DTO with tenant info.
/// </summary>
public record UserOnboardingStatusDto
{
    public Guid Id { get; init; }
    public string KeycloakId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string? Locale { get; init; }
    public bool IsActive { get; init; }
    public Guid? DefaultTenantId { get; init; }
    public bool NeedsOnboarding { get; init; }
    public bool NeedsTenantSelection { get; init; }
    public List<TenantSummaryDto> Tenants { get; init; } = [];
    public List<string> Permissions { get; init; } = [];
    public List<string> Roles { get; init; } = [];
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public record TenantSummaryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
}
