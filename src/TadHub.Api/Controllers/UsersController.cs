using Identity.Contracts;
using Identity.Contracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TadHub.Infrastructure.Auth;
using TadHub.SharedKernel.Api;

namespace TadHub.Api.Controllers;

/// <summary>
/// User profile management endpoints.
/// </summary>
[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly CurrentUser _currentUser;

    public UsersController(IIdentityService identityService, CurrentUser currentUser)
    {
        _identityService = identityService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Gets the current authenticated user's profile.
    /// Creates the profile if it doesn't exist (JIT provisioning).
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        // JIT provisioning: get or create user profile from JWT claims
        var result = await _identityService.GetOrCreateFromKeycloakAsync(
            _currentUser.KeycloakId,
            _currentUser.Email,
            _currentUser.FirstName,
            _currentUser.LastName,
            ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Updates the current user's profile.
    /// </summary>
    [HttpPatch("me")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateCurrentUser(
        [FromBody] UpdateUserProfileRequest request,
        CancellationToken ct)
    {
        // First ensure the user exists
        var existing = await _identityService.GetByKeycloakIdAsync(_currentUser.KeycloakId, ct);
        if (!existing.IsSuccess)
            return NotFound(new { error = "User profile not found" });

        var result = await _identityService.UpdateAsync(existing.Value!.Id, request, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Lists user profiles with filtering, sorting, and pagination.
    /// Requires platform-admin or tenant-admin role.
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "platform-admin,tenant-admin")]
    [ProducesResponseType(typeof(IEnumerable<UserProfileDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListUsers(
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _identityService.ListAsync(qp, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets a user profile by ID.
    /// Requires platform-admin or tenant-admin role.
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "platform-admin,tenant-admin")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(Guid id, CancellationToken ct)
    {
        var result = await _identityService.GetByIdAsync(id, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new user profile.
    /// Requires platform-admin role.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "platform-admin")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUser(
        [FromBody] CreateUserProfileRequest request,
        CancellationToken ct)
    {
        var result = await _identityService.CreateAsync(request, ct);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "CONFLICT")
                return Conflict(new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(
            nameof(GetUserById),
            new { id = result.Value!.Id },
            result.Value);
    }

    /// <summary>
    /// Updates a user profile by ID.
    /// Requires platform-admin role.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "platform-admin")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(
        Guid id,
        [FromBody] UpdateUserProfileRequest request,
        CancellationToken ct)
    {
        var result = await _identityService.UpdateAsync(id, request, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Deactivates a user profile.
    /// Requires platform-admin role.
    /// </summary>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Roles = "platform-admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateUser(Guid id, CancellationToken ct)
    {
        var result = await _identityService.DeactivateAsync(id, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }

    /// <summary>
    /// Reactivates a user profile.
    /// Requires platform-admin role.
    /// </summary>
    [HttpPost("{id:guid}/reactivate")]
    [Authorize(Roles = "platform-admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReactivateUser(Guid id, CancellationToken ct)
    {
        var result = await _identityService.ReactivateAsync(id, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }
}
