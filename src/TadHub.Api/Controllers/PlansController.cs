using Microsoft.AspNetCore.Mvc;
using TadHub.SharedKernel.Api;
using Subscription.Contracts;
using Subscription.Contracts.DTOs;

namespace TadHub.Api.Controllers;

/// <summary>
/// Subscription plan endpoints (public).
/// </summary>
[ApiController]
[Route("api/v1/plans")]
public class PlansController : ControllerBase
{
    private readonly IPlanService _planService;

    public PlansController(IPlanService planService)
    {
        _planService = planService;
    }

    /// <summary>
    /// Lists available subscription plans.
    /// Supports: filter[isActive]=true, sort=displayOrder
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlans([FromQuery] QueryParameters qp, CancellationToken ct)
    {
        var result = await _planService.GetPlansAsync(qp, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets a plan by ID.
    /// </summary>
    [HttpGet("{planId:guid}")]
    [ProducesResponseType(typeof(PlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlan(Guid planId, CancellationToken ct)
    {
        var result = await _planService.GetPlanByIdAsync(planId, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets a plan by slug.
    /// </summary>
    [HttpGet("by-slug/{slug}")]
    [ProducesResponseType(typeof(PlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlanBySlug(string slug, CancellationToken ct)
    {
        var result = await _planService.GetPlanBySlugAsync(slug, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }
}
