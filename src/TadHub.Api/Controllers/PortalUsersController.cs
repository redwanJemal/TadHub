using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Contracts;
using Portal.Contracts.DTOs;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
using TadHub.SharedKernel.Api;

namespace TadHub.Api.Controllers;

/// <summary>
/// Portal user management endpoints (tenant admin).
/// </summary>
[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/portals/{portalId:guid}/users")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class PortalUsersController : ControllerBase
{
    private readonly IPortalUserService _portalUserService;
    private readonly IPortalService _portalService;

    public PortalUsersController(IPortalUserService portalUserService, IPortalService portalService)
    {
        _portalUserService = portalUserService;
        _portalService = portalService;
    }

    /// <summary>
    /// Lists users for a portal.
    /// Supports: filter[email][contains]=..., filter[isActive]=true, sort=-createdAt
    /// </summary>
    [HttpGet]
    [HasPermission("portal.view")]
    [ProducesResponseType(typeof(IEnumerable<PortalUserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUsers(
        Guid tenantId,
        Guid portalId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        // Verify portal belongs to tenant
        var portalResult = await _portalService.GetPortalByIdAsync(tenantId, portalId, ct);
        if (!portalResult.IsSuccess)
            return NotFound(new { error = "Portal not found" });

        var result = await _portalUserService.GetUsersAsync(portalId, qp, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets a portal user by ID.
    /// </summary>
    [HttpGet("{userId:guid}")]
    [HasPermission("portal.view")]
    [ProducesResponseType(typeof(PortalUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUser(
        Guid tenantId,
        Guid portalId,
        Guid userId,
        CancellationToken ct)
    {
        var result = await _portalUserService.GetUserByIdAsync(portalId, userId, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a portal user (admin).
    /// </summary>
    [HttpPost]
    [HasPermission("portal.manage")]
    [ProducesResponseType(typeof(PortalUserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUser(
        Guid tenantId,
        Guid portalId,
        [FromBody] CreatePortalUserRequest request,
        CancellationToken ct)
    {
        var result = await _portalUserService.CreateUserAsync(portalId, request, ct);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "CONFLICT")
                return Conflict(new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(
            nameof(GetUser),
            new { tenantId, portalId, userId = result.Value!.Id },
            result.Value);
    }

    /// <summary>
    /// Updates a portal user.
    /// </summary>
    [HttpPatch("{userId:guid}")]
    [HasPermission("portal.manage")]
    [ProducesResponseType(typeof(PortalUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUser(
        Guid tenantId,
        Guid portalId,
        Guid userId,
        [FromBody] UpdatePortalUserRequest request,
        CancellationToken ct)
    {
        var result = await _portalUserService.UpdateUserAsync(portalId, userId, request, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Deletes a portal user.
    /// </summary>
    [HttpDelete("{userId:guid}")]
    [HasPermission("portal.manage")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(
        Guid tenantId,
        Guid portalId,
        Guid userId,
        CancellationToken ct)
    {
        var result = await _portalUserService.DeleteUserAsync(portalId, userId, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }
}

/// <summary>
/// Public portal user endpoints (registration, login).
/// These endpoints use the /portal/v1/ namespace.
/// </summary>
[ApiController]
[Route("portal/v1/{subdomain}")]
public class PortalPublicController : ControllerBase
{
    private readonly IPortalService _portalService;
    private readonly IPortalUserService _portalUserService;

    public PortalPublicController(IPortalService portalService, IPortalUserService portalUserService)
    {
        _portalService = portalService;
        _portalUserService = portalUserService;
    }

    /// <summary>
    /// Gets public portal info.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PortalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPortalInfo(string subdomain, CancellationToken ct)
    {
        var result = await _portalService.GetPortalBySubdomainAsync(subdomain, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Registers a new user in the portal.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(PortalUserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        string subdomain,
        [FromBody] PortalUserRegistrationRequest request,
        CancellationToken ct)
    {
        var portalResult = await _portalService.GetPortalBySubdomainAsync(subdomain, ct);
        if (!portalResult.IsSuccess)
            return NotFound(new { error = "Portal not found" });

        var result = await _portalUserService.RegisterAsync(portalResult.Value!.Id, request, ct);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "CONFLICT")
                return Conflict(new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }

        return Created($"/portal/v1/{subdomain}/me", result.Value);
    }

    /// <summary>
    /// Authenticates a portal user.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(PortalUserLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(
        string subdomain,
        [FromBody] PortalUserLoginRequest request,
        CancellationToken ct)
    {
        var portalResult = await _portalService.GetPortalBySubdomainAsync(subdomain, ct);
        if (!portalResult.IsSuccess)
            return NotFound(new { error = "Portal not found" });

        var result = await _portalUserService.LoginAsync(portalResult.Value!.Id, request, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Verifies email with token.
    /// </summary>
    [HttpPost("verify-email")]
    [ProducesResponseType(typeof(PortalUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyEmail(
        string subdomain,
        [FromBody] VerifyEmailRequest request,
        CancellationToken ct)
    {
        var result = await _portalUserService.VerifyEmailAsync(request.Token, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }
}

/// <summary>
/// Request to verify email.
/// </summary>
public record VerifyEmailRequest
{
    public string Token { get; init; } = string.Empty;
}
