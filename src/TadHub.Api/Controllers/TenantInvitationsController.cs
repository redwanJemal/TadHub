using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TadHub.Infrastructure.Auth;
using TadHub.Api.Filters;
using TadHub.SharedKernel.Api;
using Tenancy.Contracts;
using Tenancy.Contracts.DTOs;

namespace TadHub.Api.Controllers;

/// <summary>
/// Tenant invitation management endpoints.
/// </summary>
[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/invitations")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class TenantInvitationsController : ControllerBase
{
    private readonly ITenantService _tenantService;

    public TenantInvitationsController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    /// <summary>
    /// Lists pending invitations for a tenant.
    /// </summary>
    [HttpGet]
    [HasPermission("members.invite")]
    [ProducesResponseType(typeof(IEnumerable<TenantInvitationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvitations(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _tenantService.GetInvitationsAsync(tenantId, qp, ct);
        return Ok(result);
    }

    /// <summary>
    /// Creates an invitation to join the tenant.
    /// </summary>
    [HttpPost]
    [HasPermission("members.invite")]
    [ProducesResponseType(typeof(TenantInvitationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> InviteMember(
        Guid tenantId,
        [FromBody] InviteMemberRequest request,
        CancellationToken ct)
    {
        var result = await _tenantService.InviteMemberAsync(tenantId, request, ct);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "CONFLICT")
                return Conflict(new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }

        return Created($"/api/v1/tenants/{tenantId}/invitations/{result.Value!.Id}", result.Value);
    }

    /// <summary>
    /// Revokes a pending invitation.
    /// </summary>
    [HttpDelete("{invitationId:guid}")]
    [HasPermission("members.invite")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeInvitation(
        Guid tenantId,
        Guid invitationId,
        CancellationToken ct)
    {
        var result = await _tenantService.RevokeInvitationAsync(tenantId, invitationId, ct);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "VALIDATION_ERROR")
                return BadRequest(new { error = result.Error });
            return NotFound(new { error = result.Error });
        }

        return NoContent();
    }
}

/// <summary>
/// Public invitation acceptance endpoint (no tenant context required).
/// </summary>
[ApiController]
[Route("api/v1/invitations")]
public class InvitationsController : ControllerBase
{
    private readonly ITenantService _tenantService;

    public InvitationsController(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    /// <summary>
    /// Gets invitation details by token (public, for showing invitation page).
    /// </summary>
    [HttpGet("{token}")]
    [ProducesResponseType(typeof(TenantInvitationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByToken(string token, CancellationToken ct)
    {
        var result = await _tenantService.GetInvitationByTokenAsync(token, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Accepts an invitation using the token.
    /// Requires authentication.
    /// </summary>
    [HttpPost("{token}/accept")]
    [Authorize]
    [ProducesResponseType(typeof(TenantMemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcceptInvitation(string token, CancellationToken ct)
    {
        var result = await _tenantService.AcceptInvitationAsync(token, ct);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "VALIDATION_ERROR")
                return BadRequest(new { error = result.Error });
            if (result.ErrorCode == "CONFLICT")
                return Conflict(new { error = result.Error });
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}
