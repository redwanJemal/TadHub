using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Portal.Contracts;
using Portal.Contracts.DTOs;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
using TadHub.SharedKernel.Api;

namespace TadHub.Api.Controllers;

/// <summary>
/// Portal management endpoints (tenant admin).
/// </summary>
[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/portals")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class PortalsController : ControllerBase
{
    private readonly IPortalService _portalService;

    public PortalsController(IPortalService portalService)
    {
        _portalService = portalService;
    }

    /// <summary>
    /// Lists portals for a tenant.
    /// Supports: filter[isActive]=true, sort=name
    /// </summary>
    [HttpGet]
    [HasPermission("portal.view")]
    [ProducesResponseType(typeof(IEnumerable<PortalDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPortals(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _portalService.GetPortalsAsync(tenantId, qp, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets a portal by ID.
    /// </summary>
    [HttpGet("{portalId:guid}")]
    [HasPermission("portal.view")]
    [ProducesResponseType(typeof(PortalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPortal(Guid tenantId, Guid portalId, CancellationToken ct)
    {
        var result = await _portalService.GetPortalByIdAsync(tenantId, portalId, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new portal.
    /// </summary>
    [HttpPost]
    [HasPermission("portal.manage")]
    [ProducesResponseType(typeof(PortalDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreatePortal(
        Guid tenantId,
        [FromBody] CreatePortalRequest request,
        CancellationToken ct)
    {
        var result = await _portalService.CreatePortalAsync(tenantId, request, ct);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "CONFLICT")
                return Conflict(new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }

        return CreatedAtAction(
            nameof(GetPortal),
            new { tenantId, portalId = result.Value!.Id },
            result.Value);
    }

    /// <summary>
    /// Updates a portal.
    /// </summary>
    [HttpPatch("{portalId:guid}")]
    [HasPermission("portal.manage")]
    [ProducesResponseType(typeof(PortalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePortal(
        Guid tenantId,
        Guid portalId,
        [FromBody] UpdatePortalRequest request,
        CancellationToken ct)
    {
        var result = await _portalService.UpdatePortalAsync(tenantId, portalId, request, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Deletes a portal.
    /// </summary>
    [HttpDelete("{portalId:guid}")]
    [HasPermission("portal.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePortal(Guid tenantId, Guid portalId, CancellationToken ct)
    {
        var result = await _portalService.DeletePortalAsync(tenantId, portalId, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }

    /// <summary>
    /// Adds a custom domain to a portal.
    /// </summary>
    [HttpPost("{portalId:guid}/domains")]
    [HasPermission("portal.manage")]
    [ProducesResponseType(typeof(PortalDomainDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddDomain(
        Guid tenantId,
        Guid portalId,
        [FromBody] AddDomainRequest request,
        CancellationToken ct)
    {
        var result = await _portalService.AddDomainAsync(tenantId, portalId, request.Domain, ct);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "CONFLICT")
                return Conflict(new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }

        return Created($"/api/v1/tenants/{tenantId}/portals/{portalId}/domains/{result.Value!.Id}", result.Value);
    }

    /// <summary>
    /// Verifies a custom domain.
    /// </summary>
    [HttpPost("{portalId:guid}/domains/{domainId:guid}/verify")]
    [HasPermission("portal.manage")]
    [ProducesResponseType(typeof(PortalDomainDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VerifyDomain(
        Guid tenantId,
        Guid portalId,
        Guid domainId,
        CancellationToken ct)
    {
        var result = await _portalService.VerifyDomainAsync(tenantId, portalId, domainId, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Removes a custom domain.
    /// </summary>
    [HttpDelete("{portalId:guid}/domains/{domainId:guid}")]
    [HasPermission("portal.manage")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveDomain(
        Guid tenantId,
        Guid portalId,
        Guid domainId,
        CancellationToken ct)
    {
        var result = await _portalService.RemoveDomainAsync(tenantId, portalId, domainId, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }
}

/// <summary>
/// Request to add a domain.
/// </summary>
public record AddDomainRequest
{
    public string Domain { get; init; } = string.Empty;
}
