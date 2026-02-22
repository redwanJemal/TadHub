using Identity.Contracts;
using Identity.Contracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TadHub.SharedKernel.Api;

namespace TadHub.Api.Controllers;

/// <summary>
/// Platform staff management endpoints.
/// These endpoints manage users who have access to the backoffice/admin panel.
/// </summary>
[ApiController]
[Route("api/v1/platform/staff")]
[Authorize(Roles = "platform-admin")]
public class PlatformStaffController : ControllerBase
{
    private readonly IPlatformStaffService _staffService;

    public PlatformStaffController(IPlatformStaffService staffService)
    {
        _staffService = staffService;
    }

    /// <summary>
    /// Lists all platform staff with pagination and filtering.
    /// </summary>
    /// <remarks>
    /// Filters:
    /// - role: Filter by platform role (e.g., "super-admin", "admin", "finance", "sales", "support")
    ///
    /// Sort fields: createdAt, updatedAt, email, firstName, lastName, role
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PlatformStaffDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListStaff(
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _staffService.ListAsync(qp, ct);

        // Add pagination headers
        Response.Headers.Append("X-Total-Count", result.TotalCount.ToString());
        Response.Headers.Append("X-Total-Pages", result.TotalPages.ToString());
        Response.Headers.Append("X-Page", result.Page.ToString());
        Response.Headers.Append("X-Page-Size", result.PageSize.ToString());

        return Ok(result.Items);
    }

    /// <summary>
    /// Gets a platform staff member by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PlatformStaffDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStaff(Guid id, CancellationToken ct)
    {
        var result = await _staffService.GetByIdAsync(id, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new platform staff member.
    /// The user must already exist (have logged in at least once).
    /// </summary>
    /// <remarks>
    /// To make a user platform staff:
    /// 1. The user must first log in via Keycloak (creates UserProfile)
    /// 2. Then call this endpoint with their email
    ///
    /// This creates a PlatformStaff record linked to their UserProfile.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType(typeof(PlatformStaffDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateStaff(
        [FromBody] CreatePlatformStaffRequest request,
        CancellationToken ct)
    {
        var result = await _staffService.CreateAsync(request, ct);

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
            nameof(GetStaff),
            new { id = result.Value!.Id },
            result.Value);
    }

    /// <summary>
    /// Updates a platform staff member.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(PlatformStaffDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStaff(
        Guid id,
        [FromBody] UpdatePlatformStaffRequest request,
        CancellationToken ct)
    {
        var result = await _staffService.UpdateAsync(id, request, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Removes platform staff status from a user.
    /// This deletes the PlatformStaff record but keeps the UserProfile.
    /// </summary>
    /// <remarks>
    /// Cannot remove the last super-admin.
    /// </remarks>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveStaff(Guid id, CancellationToken ct)
    {
        var result = await _staffService.DeleteAsync(id, ct);

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
    /// Gets the current user's platform staff record if they are platform staff.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(PlatformStaffDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentStaff(
        [FromServices] TadHub.Infrastructure.Auth.CurrentUser currentUser,
        CancellationToken ct)
    {
        var result = await _staffService.GetByUserIdAsync(currentUser.UserId, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = "Current user is not platform staff" });

        return Ok(result.Value);
    }
}
