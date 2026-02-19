using ClientManagement.Contracts;
using ClientManagement.Contracts.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TadHub.SharedKernel.Api;

namespace ClientManagement.Api.Controllers;

/// <summary>
/// Lead management endpoints.
/// </summary>
[ApiController]
[Route("api/v1/leads")]
[Authorize]
public class LeadsController : ControllerBase
{
    private readonly ILeadService _leadService;

    public LeadsController(ILeadService leadService)
    {
        _leadService = leadService;
    }

    /// <summary>
    /// Creates a new lead.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "leads.create")]
    [ProducesResponseType(typeof(LeadDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateLeadRequest request, CancellationToken ct)
    {
        var result = await _leadService.CreateAsync(request, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error, message = result.Error });

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Gets a lead by ID.
    /// </summary>
    /// <param name="id">Lead ID</param>
    /// <param name="include">Relations to include: client</param>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "leads.view")]
    [ProducesResponseType(typeof(LeadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] string? include, CancellationToken ct)
    {
        var includes = IncludeResolver.Parse(include);
        var result = await _leadService.GetByIdAsync(id, includes, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error, message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Updates a lead.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [Authorize(Policy = "leads.update")]
    [ProducesResponseType(typeof(LeadDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLeadRequest request, CancellationToken ct)
    {
        var result = await _leadService.UpdateAsync(id, request, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error, message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Converts a lead to a client.
    /// </summary>
    /// <remarks>
    /// Creates the client, links the lead, and sets lead status to Converted.
    /// Returns the newly created client.
    /// </remarks>
    [HttpPost("{id:guid}/convert")]
    [Authorize(Policy = "leads.manage")]
    [Authorize(Policy = "clients.create")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ConvertToClient(Guid id, [FromBody] ConvertLeadRequest request, CancellationToken ct)
    {
        var result = await _leadService.ConvertToClientAsync(id, request, ct);

        if (!result.IsSuccess)
        {
            return result.Error switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error, message = result.Error }),
                "ALREADY_CONVERTED" => BadRequest(new { error = result.Error, message = result.Error }),
                _ => BadRequest(new { error = result.Error, message = result.Error })
            };
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Lists leads with filtering, sorting, and pagination.
    /// </summary>
    /// <remarks>
    /// Filters:
    /// - filter[status]=new,contacted (array)
    /// - filter[source]=walkIn
    /// - filter[assignedToUserId]=...
    /// - filter[createdAt][gte]=2026-01-01
    /// 
    /// Sorting:
    /// - sort=createdAt:desc
    /// - sort=status:asc
    /// 
    /// Includes:
    /// - include=client (only useful for converted leads)
    /// </remarks>
    [HttpGet]
    [Authorize(Policy = "leads.view")]
    [ProducesResponseType(typeof(PagedList<LeadDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] QueryParameters query, CancellationToken ct)
    {
        var result = await _leadService.ListAsync(query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets lead conversion funnel statistics.
    /// </summary>
    /// <param name="from">Start date filter (optional)</param>
    /// <param name="to">End date filter (optional)</param>
    [HttpGet("stats/funnel")]
    [Authorize(Policy = "leads.view")]
    [ProducesResponseType(typeof(LeadFunnelStats), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFunnelStats([FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, CancellationToken ct)
    {
        var result = await _leadService.GetFunnelStatsAsync(from, to, ct);
        return Ok(result);
    }
}
