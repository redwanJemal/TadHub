using Authorization.Contracts;
using Authorization.Contracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TadHub.SharedKernel.Api;

namespace TadHub.Api.Controllers;

/// <summary>
/// Permission listing endpoints.
/// Permissions are global and predefined - this is read-only.
/// </summary>
[ApiController]
[Route("api/v1/permissions")]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly IAuthorizationModuleService _authService;

    public PermissionsController(IAuthorizationModuleService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Lists all permissions with optional filtering by module.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PermissionDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPermissions(
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _authService.GetPermissionsAsync(qp, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets a permission by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PermissionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPermission(Guid id, CancellationToken ct)
    {
        var result = await _authService.GetPermissionByIdAsync(id, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}
