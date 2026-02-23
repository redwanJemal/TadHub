using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;
using Tenancy.Contracts;
using Tenancy.Contracts.DTOs;

namespace TadHub.Api.Controllers;

/// <summary>
/// Platform admin tenant management endpoints.
/// These endpoints allow platform admins to manage all tenants without tenant context.
/// </summary>
[ApiController]
[Route("api/v1/admin/tenants")]
[Authorize(Roles = "platform-admin")]
public class AdminTenantsController : ControllerBase
{
    private readonly ITenantService _tenantService;

    public AdminTenantsController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    #region Tenant Operations

    /// <summary>
    /// Lists all tenants with pagination and filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedList<TenantDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListTenants(
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _tenantService.ListAllTenantsAsync(qp, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets a tenant by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTenant(Guid id, CancellationToken ct)
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
    public async Task<IActionResult> GetTenantBySlug(string slug, CancellationToken ct)
    {
        var result = await _tenantService.GetBySlugAsync(slug, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new tenant with a dedicated owner user.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateTenant(
        [FromBody] AdminCreateTenantRequest request,
        CancellationToken ct)
    {
        var result = await _tenantService.AdminCreateAsync(request, ct);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "CONFLICT" => Conflict(new { error = result.Error }),
                "UNAUTHORIZED" => Unauthorized(new { error = result.Error }),
                "KEYCLOAK_ERROR" => StatusCode(StatusCodes.Status502BadGateway, new { error = result.Error }),
                "IDENTITY_ERROR" => StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error }),
                _ => BadRequest(new { error = result.Error })
            };
        }

        return CreatedAtAction(
            nameof(GetTenant),
            new { id = result.Value!.Id },
            result.Value);
    }

    /// <summary>
    /// Updates a tenant.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTenant(
        Guid id,
        [FromBody] UpdateTenantRequest request,
        CancellationToken ct)
    {
        var result = await _tenantService.UpdateAsync(id, request, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Suspends a tenant.
    /// </summary>
    [HttpPost("{id:guid}/suspend")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuspendTenant(Guid id, CancellationToken ct)
    {
        var result = await _tenantService.SuspendAsync(id, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }

    /// <summary>
    /// Reactivates a suspended tenant.
    /// </summary>
    [HttpPost("{id:guid}/reactivate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReactivateTenant(Guid id, CancellationToken ct)
    {
        var result = await _tenantService.ReactivateAsync(id, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }

    /// <summary>
    /// Deletes a tenant.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTenant(Guid id, CancellationToken ct)
    {
        var result = await _tenantService.DeleteAsync(id, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }

    #endregion

    #region Member Operations

    /// <summary>
    /// Gets members of a tenant.
    /// </summary>
    [HttpGet("{id:guid}/members")]
    [ProducesResponseType(typeof(PagedList<TenantMemberDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMembers(
        Guid id,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var tenant = await _tenantService.GetByIdAsync(id, ct);
        if (!tenant.IsSuccess)
            return NotFound(new { error = tenant.Error });

        var result = await _tenantService.GetMembersAsync(id, qp, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific member of a tenant.
    /// </summary>
    [HttpGet("{tenantId:guid}/members/{userId:guid}")]
    [ProducesResponseType(typeof(TenantMemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMember(
        Guid tenantId,
        Guid userId,
        CancellationToken ct)
    {
        var result = await _tenantService.GetMemberAsync(tenantId, userId, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Adds a user as a member of a tenant.
    /// </summary>
    [HttpPost("{tenantId:guid}/members")]
    [ProducesResponseType(typeof(TenantMemberDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddMember(
        Guid tenantId,
        [FromBody] AdminAddMemberRequest request,
        CancellationToken ct)
    {
        var result = await _tenantService.AddMemberAsync(tenantId, request.UserId, request.IsOwner, ct);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error }),
                "CONFLICT" => Conflict(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error })
            };
        }

        return CreatedAtAction(
            nameof(GetMember),
            new { tenantId, userId = request.UserId },
            result.Value);
    }

    /// <summary>
    /// Removes a member from a tenant.
    /// </summary>
    [HttpDelete("{tenantId:guid}/members/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(
        Guid tenantId,
        Guid userId,
        CancellationToken ct)
    {
        var result = await _tenantService.RemoveMemberAsync(tenantId, userId, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }

    #endregion

    #region Invitation Operations

    /// <summary>
    /// Gets pending invitations for a tenant.
    /// </summary>
    [HttpGet("{tenantId:guid}/invitations")]
    [ProducesResponseType(typeof(PagedList<TenantInvitationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvitations(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _tenantService.GetInvitationsAsync(tenantId, qp, ct);
        return Ok(result);
    }

    /// <summary>
    /// Creates an invitation to join a tenant.
    /// </summary>
    [HttpPost("{tenantId:guid}/invitations")]
    [ProducesResponseType(typeof(TenantInvitationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateInvitation(
        Guid tenantId,
        [FromBody] InviteMemberRequest request,
        CancellationToken ct)
    {
        var result = await _tenantService.InviteMemberAsync(tenantId, request, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Created($"/api/v1/admin/tenants/{tenantId}/invitations/{result.Value!.Id}", result.Value);
    }

    /// <summary>
    /// Revokes a pending invitation.
    /// </summary>
    [HttpDelete("{tenantId:guid}/invitations/{invitationId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeInvitation(
        Guid tenantId,
        Guid invitationId,
        CancellationToken ct)
    {
        var result = await _tenantService.RevokeInvitationAsync(tenantId, invitationId, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }

    #endregion
}

/// <summary>
/// Request to add a member to a tenant (admin endpoint).
/// </summary>
public record AdminAddMemberRequest(Guid UserId, bool IsOwner = false);
