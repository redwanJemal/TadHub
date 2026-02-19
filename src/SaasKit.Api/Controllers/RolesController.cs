using Authorization.Contracts;
using Authorization.Contracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaasKit.Api.Filters;
using SaasKit.Infrastructure.Auth;
using SaasKit.SharedKernel.Api;
using SaasKit.SharedKernel.Interfaces;

namespace SaasKit.Api.Controllers;

/// <summary>
/// Role management endpoints.
/// </summary>
[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/roles")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class RolesController : ControllerBase
{
    private readonly IAuthorizationModuleService _authService;
    private readonly ICurrentUser _currentUser;

    public RolesController(IAuthorizationModuleService authService, ICurrentUser currentUser)
    {
        _authService = authService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Lists roles for a tenant with filtering.
    /// </summary>
    [HttpGet]
    [HasPermission("roles.view")]
    [ProducesResponseType(typeof(IEnumerable<RoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoles(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _authService.GetRolesAsync(tenantId, qp, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets a role by ID.
    /// </summary>
    [HttpGet("{roleId:guid}")]
    [HasPermission("roles.view")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRole(Guid tenantId, Guid roleId, CancellationToken ct)
    {
        var result = await _authService.GetRoleByIdAsync(tenantId, roleId, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new role.
    /// </summary>
    [HttpPost]
    [HasPermission("roles.manage")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateRole(
        Guid tenantId,
        [FromBody] CreateRoleRequest request,
        CancellationToken ct)
    {
        var result = await _authService.CreateRoleAsync(tenantId, request, ct);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "CONFLICT")
                return Conflict(new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(
            nameof(GetRole),
            new { tenantId, roleId = result.Value!.Id },
            result.Value);
    }

    /// <summary>
    /// Updates a role.
    /// </summary>
    [HttpPatch("{roleId:guid}")]
    [HasPermission("roles.manage")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRole(
        Guid tenantId,
        Guid roleId,
        [FromBody] UpdateRoleRequest request,
        CancellationToken ct)
    {
        var result = await _authService.UpdateRoleAsync(tenantId, roleId, request, ct);

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

    /// <summary>
    /// Deletes a role.
    /// </summary>
    [HttpDelete("{roleId:guid}")]
    [HasPermission("roles.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRole(Guid tenantId, Guid roleId, CancellationToken ct)
    {
        var result = await _authService.DeleteRoleAsync(tenantId, roleId, ct);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "VALIDATION_ERROR")
                return BadRequest(new { error = result.Error });
            return NotFound(new { error = result.Error });
        }

        return NoContent();
    }

    /// <summary>
    /// Gets roles assigned to the current user in this tenant.
    /// </summary>
    [HttpGet("my-roles")]
    [ProducesResponseType(typeof(IEnumerable<RoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyRoles(Guid tenantId, CancellationToken ct)
    {
        var result = await _authService.GetUserRolesAsync(tenantId, _currentUser.UserId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    [HttpPost("assign")]
    [HasPermission("roles.assign")]
    [ProducesResponseType(typeof(UserRoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssignRole(
        Guid tenantId,
        [FromBody] AssignRoleRequest request,
        CancellationToken ct)
    {
        var result = await _authService.AssignRoleAsync(tenantId, request, ct);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "CONFLICT")
                return Conflict(new { error = result.Error });
            return NotFound(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Removes a role from a user.
    /// </summary>
    [HttpDelete("users/{userId:guid}/roles/{roleId:guid}")]
    [HasPermission("roles.assign")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveRole(
        Guid tenantId,
        Guid userId,
        Guid roleId,
        CancellationToken ct)
    {
        var result = await _authService.RemoveRoleAsync(tenantId, userId, roleId, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }
}
