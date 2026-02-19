using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaasKit.Infrastructure.Auth;
using SaasKit.Api.Filters;
using SaasKit.Infrastructure.Tenancy;
using SaasKit.SharedKernel.Api;
using Tenancy.Contracts;
using Tenancy.Contracts.DTOs;

namespace SaasKit.Api.Controllers;

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
    /// Lists tenants the current user is a member of.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TenantDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListUserTenants(
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
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
    /// Updates a tenant.
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
        // Check if user is admin or owner
        var role = await _tenantService.GetUserRoleAsync(id, _currentUser.UserId, ct);
        if (role is not (TenantRole.Admin or TenantRole.Owner))
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
        var role = await _tenantService.GetUserRoleAsync(id, _currentUser.UserId, ct);
        if (role != TenantRole.Owner)
            return Forbid();

        var result = await _tenantService.DeleteAsync(id, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }
}
