using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TadHub.Infrastructure.Auth;
using TadHub.Api.Filters;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Interfaces;
using Tenancy.Contracts;
using Tenancy.Contracts.DTOs;

namespace TadHub.Api.Controllers;

/// <summary>
/// Tenant membership management endpoints.
/// </summary>
[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/members")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class TenantMembersController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly CurrentUser _currentUser;
    private readonly IPermissionChecker _permissionChecker;

    public TenantMembersController(
        ITenantService tenantService,
        CurrentUser currentUser,
        IPermissionChecker permissionChecker)
    {
        _tenantService = tenantService;
        _currentUser = currentUser;
        _permissionChecker = permissionChecker;
    }

    /// <summary>
    /// Lists members of a tenant with filtering and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TenantMemberDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMembers(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var hasPermission = await _permissionChecker.HasPermissionAsync(
            tenantId, _currentUser.UserId, "members.view", ct);
        if (!hasPermission)
            return Forbid();

        var result = await _tenantService.GetMembersAsync(tenantId, qp, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific member.
    /// </summary>
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(TenantMemberDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMember(Guid tenantId, Guid userId, CancellationToken ct)
    {
        var hasPermission = await _permissionChecker.HasPermissionAsync(
            tenantId, _currentUser.UserId, "members.view", ct);
        if (!hasPermission)
            return Forbid();

        var result = await _tenantService.GetMemberAsync(tenantId, userId, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Removes a member from the tenant.
    /// Allows self-removal without permission, otherwise requires members.remove permission.
    /// </summary>
    [HttpDelete("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(Guid tenantId, Guid userId, CancellationToken ct)
    {
        // Allow self-removal; otherwise require members.remove permission
        var isSelf = userId == _currentUser.UserId;
        if (!isSelf)
        {
            var hasPermission = await _permissionChecker.HasPermissionAsync(
                tenantId, _currentUser.UserId, "members.remove", ct);
            if (!hasPermission)
                return Forbid();
        }

        var result = await _tenantService.RemoveMemberAsync(tenantId, userId, ct);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "VALIDATION_ERROR")
                return BadRequest(new { error = result.Error });
            return NotFound(new { error = result.Error });
        }

        return NoContent();
    }
}
