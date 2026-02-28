using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Notification.Contracts.Settings;
using TadHub.Infrastructure.Auth;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Tenancy;
using TadHub.SharedKernel.Api;
using Tenancy.Contracts;
using Tenancy.Contracts.DTOs;

namespace TadHub.Api.Controllers;

/// <summary>
/// Tenant management endpoints.
/// </summary>
[ApiController]
[Route("api/v1/tenants")]
[Authorize]
public class TenantsController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly CurrentUser _currentUser;

    public TenantsController(ITenantService tenantService, CurrentUser currentUser)
    {
        _tenantService = tenantService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Lists tenants. Platform admins see all tenants, regular users see only their tenants.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TenantDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListTenants(
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        // Platform admins can see all tenants
        if (User.IsInRole("platform-admin"))
        {
            var allResult = await _tenantService.ListAllTenantsAsync(qp, ct);
            return Ok(allResult);
        }

        // Regular users see only their tenants
        var result = await _tenantService.ListUserTenantsAsync(_currentUser.UserId, qp, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets a tenant by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [TenantMemberRequired]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _tenantService.GetByIdAsync(id, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets a tenant by slug.
    /// </summary>
    [HttpGet("by-slug/{slug}")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken ct)
    {
        var result = await _tenantService.GetBySlugAsync(slug, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        // Check if user is a member
        var isMember = await _tenantService.IsMemberAsync(result.Value!.Id, _currentUser.UserId, ct);
        if (!isMember)
            return Forbid();

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new tenant. The current user becomes the owner.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTenantRequest request,
        CancellationToken ct)
    {
        var result = await _tenantService.CreateAsync(request, ct);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "CONFLICT")
                return Conflict(new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            result.Value);
    }

    /// <summary>
    /// Updates a tenant. Requires ownership.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [TenantMemberRequired]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTenantRequest request,
        CancellationToken ct)
    {
        // Check if user is owner (use RBAC for finer-grained checks)
        var isOwner = await _tenantService.IsOwnerAsync(id, _currentUser.UserId, ct);
        if (!isOwner)
            return Forbid();

        var result = await _tenantService.UpdateAsync(id, request, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Suspends a tenant (platform-admin only).
    /// </summary>
    [HttpPost("{id:guid}/suspend")]
    [Authorize(Roles = "platform-admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken ct)
    {
        var result = await _tenantService.SuspendAsync(id, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }

    /// <summary>
    /// Reactivates a suspended tenant (platform-admin only).
    /// </summary>
    [HttpPost("{id:guid}/reactivate")]
    [Authorize(Roles = "platform-admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
    {
        var result = await _tenantService.ReactivateAsync(id, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }

    /// <summary>
    /// Gets notification settings for a tenant.
    /// </summary>
    [HttpGet("{id:guid}/settings/notifications")]
    [TenantMemberRequired]
    [ProducesResponseType(typeof(TenantNotificationSettings), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNotificationSettings(Guid id, CancellationToken ct)
    {
        var isOwner = await _tenantService.IsOwnerAsync(id, _currentUser.UserId, ct);
        if (!isOwner)
            return Forbid();

        var result = await _tenantService.GetSettingsJsonAsync(id, ct);
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        var settings = new TenantNotificationSettings();
        if (!string.IsNullOrWhiteSpace(result.Value))
        {
            var root = JsonNode.Parse(result.Value);
            var notificationsNode = root?["notifications"];
            if (notificationsNode is not null)
            {
                settings = notificationsNode.Deserialize<TenantNotificationSettings>() ?? new TenantNotificationSettings();
            }
        }

        return Ok(settings);
    }

    /// <summary>
    /// Updates notification settings for a tenant.
    /// </summary>
    [HttpPut("{id:guid}/settings/notifications")]
    [TenantMemberRequired]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateNotificationSettings(
        Guid id,
        [FromBody] TenantNotificationSettings settings,
        CancellationToken ct)
    {
        var isOwner = await _tenantService.IsOwnerAsync(id, _currentUser.UserId, ct);
        if (!isOwner)
            return Forbid();

        var sectionJson = JsonSerializer.Serialize(settings);
        var result = await _tenantService.UpdateSettingsSectionAsync(id, "notifications", sectionJson, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }

    /// <summary>
    /// Deletes a tenant (owner only).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [TenantMemberRequired]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        // Only owner can delete
        var isOwner = await _tenantService.IsOwnerAsync(id, _currentUser.UserId, ct);
        if (!isOwner)
            return Forbid();

        var result = await _tenantService.DeleteAsync(id, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }
}
