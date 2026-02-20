using Identity.Contracts;
using Identity.Contracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TadHub.SharedKernel.Api;

namespace TadHub.Api.Controllers;

/// <summary>
/// Platform admin user management endpoints.
/// These endpoints manage users who have access to the backoffice/admin panel.
/// </summary>
[ApiController]
[Route("api/v1/admin/users")]
[Authorize(Roles = "platform-admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminUsersController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    /// <summary>
    /// Lists all platform admin users with pagination and filtering.
    /// </summary>
    /// <remarks>
    /// Filters:
    /// - isSuperAdmin: Filter by super admin status (true/false)
    /// 
    /// Sort fields: createdAt, updatedAt, email, firstName, lastName
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AdminUserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListAdminUsers(
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _adminUserService.ListAsync(qp, ct);
        
        // Add pagination headers
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        Response.Headers.Append("X-Total-Pages", result.TotalPages.ToString());
        Response.Headers.Append("X-Page", result.Page.ToString());
        Response.Headers.Append("X-Page-Size", result.PageSize.ToString());
        
        return Ok(result.Items);
    }

    /// <summary>
    /// Gets a platform admin user by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAdminUser(Guid id, CancellationToken ct)
    {
        var result = await _adminUserService.GetByIdAsync(id, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new platform admin user.
    /// The user must already exist (have logged in at least once).
    /// </summary>
    /// <remarks>
    /// To make a user an admin:
    /// 1. The user must first log in via Keycloak (creates UserProfile)
    /// 2. Then call this endpoint with their email
    /// 
    /// This creates an AdminUser record linked to their UserProfile.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAdminUser(
        [FromBody] CreateAdminUserRequest request,
        CancellationToken ct)
    {
        var result = await _adminUserService.CreateAsync(request, ct);

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
            nameof(GetAdminUser),
            new { id = result.Value!.Id },
            result.Value);
    }

    /// <summary>
    /// Updates a platform admin user.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAdminUser(
        Guid id,
        [FromBody] UpdateAdminUserRequest request,
        CancellationToken ct)
    {
        var result = await _adminUserService.UpdateAsync(id, request, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Removes admin status from a user.
    /// This deletes the AdminUser record but keeps the UserProfile.
    /// </summary>
    /// <remarks>
    /// Cannot remove the last super admin.
    /// </remarks>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveAdminUser(Guid id, CancellationToken ct)
    {
        var result = await _adminUserService.DeleteAsync(id, ct);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error }),
                "VALIDATION_ERROR" => BadRequest(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error })
            };
        }

        return NoContent();
    }

    /// <summary>
    /// Gets the current user's admin record if they are a platform admin.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(AdminUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentAdminUser(
        [FromServices] TadHub.Infrastructure.Auth.CurrentUser currentUser,
        CancellationToken ct)
    {
        var result = await _adminUserService.GetByUserIdAsync(currentUser.UserId, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = "Current user is not a platform admin" });

        return Ok(result.Value);
    }
}
