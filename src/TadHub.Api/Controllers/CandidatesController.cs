using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Candidate.Contracts;
using Candidate.Contracts.DTOs;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace TadHub.Api.Controllers;

/// <summary>
/// Tenant-scoped candidate management endpoints.
/// </summary>
[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/candidates")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class CandidatesController : ControllerBase
{
    private readonly ICandidateService _candidateService;

    public CandidatesController(ICandidateService candidateService)
    {
        _candidateService = candidateService;
    }

    /// <summary>
    /// Lists candidates for this tenant with filtering, sorting, search, and pagination.
    /// </summary>
    /// <remarks>
    /// Filters:
    /// - filter[status]=Received,UnderReview
    /// - filter[sourceType]=Supplier
    /// - filter[nationality]=PH,IN
    /// - filter[tenantSupplierId]=guid
    /// - filter[gender]=Male
    /// - filter[createdBy]=guid
    /// - filter[passportNumber]=ABC123
    /// - filter[externalReference]=REF001
    ///
    /// Sort:
    /// - sort=-createdAt (default, newest first)
    /// - sort=fullNameEn
    /// - sort=status
    ///
    /// Search:
    /// - search=john (searches name, passport number, external reference)
    /// </remarks>
    [HttpGet]
    [HasPermission("candidates.view")]
    [ProducesResponseType(typeof(PagedList<CandidateListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _candidateService.ListAsync(tenantId, qp, ct);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new candidate.
    /// </summary>
    [HttpPost]
    [HasPermission("candidates.create")]
    [ProducesResponseType(typeof(CandidateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        Guid tenantId,
        [FromBody] CreateCandidateRequest request,
        CancellationToken ct)
    {
        var result = await _candidateService.CreateAsync(tenantId, request, ct);

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
            nameof(GetById),
            new { tenantId, id = result.Value!.Id },
            result.Value);
    }

    /// <summary>
    /// Gets a candidate by ID.
    /// </summary>
    /// <remarks>
    /// Include:
    /// - include=statusHistory (includes status change history)
    /// </remarks>
    [HttpGet("{id:guid}")]
    [HasPermission("candidates.view")]
    [ProducesResponseType(typeof(CandidateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid tenantId,
        Guid id,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _candidateService.GetByIdAsync(tenantId, id, qp, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Updates a candidate (partial update).
    /// </summary>
    [HttpPatch("{id:guid}")]
    [HasPermission("candidates.edit")]
    [ProducesResponseType(typeof(CandidateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid tenantId,
        Guid id,
        [FromBody] UpdateCandidateRequest request,
        CancellationToken ct)
    {
        var result = await _candidateService.UpdateAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error }),
                "CONFLICT" => Conflict(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error })
            };
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Transitions a candidate's status.
    /// </summary>
    [HttpPost("{id:guid}/status")]
    [HasPermission("candidates.manage_status")]
    [ProducesResponseType(typeof(CandidateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransitionStatus(
        Guid tenantId,
        Guid id,
        [FromBody] TransitionStatusRequest request,
        CancellationToken ct)
    {
        var result = await _candidateService.TransitionStatusAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error })
            };
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets the status history for a candidate.
    /// </summary>
    [HttpGet("{id:guid}/status-history")]
    [HasPermission("candidates.view")]
    [ProducesResponseType(typeof(List<CandidateStatusHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatusHistory(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _candidateService.GetStatusHistoryAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Soft deletes a candidate.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission("candidates.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _candidateService.DeleteAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }
}
