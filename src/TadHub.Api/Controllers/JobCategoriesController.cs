using Microsoft.AspNetCore.Mvc;
using ReferenceData.Contracts;
using ReferenceData.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace TadHub.Api.Controllers;

/// <summary>
/// MoHRE job category endpoints (public).
/// 19 official domestic worker categories.
/// </summary>
[ApiController]
[Route("api/v1/job-categories")]
public class JobCategoriesController : ControllerBase
{
    private readonly IJobCategoryService _jobCategoryService;

    public JobCategoriesController(IJobCategoryService jobCategoryService)
    {
        _jobCategoryService = jobCategoryService;
    }

    /// <summary>
    /// Lists job categories with filtering and pagination.
    /// </summary>
    /// <remarks>
    /// Filters:
    /// - filter[moHRECode]=DMW,HSK (MoHRE codes)
    /// - filter[isActive]=true
    /// 
    /// Sort:
    /// - sort=displayOrder (default)
    /// - sort=moHRECode
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedList<JobCategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] QueryParameters qp, CancellationToken ct)
    {
        var result = await _jobCategoryService.ListAsync(qp, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets all active job categories (no pagination).
    /// Use for dropdowns when you need the complete list.
    /// </summary>
    [HttpGet("all")]
    [ProducesResponseType(typeof(List<JobCategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _jobCategoryService.GetAllAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets a job category by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JobCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _jobCategoryService.GetByIdAsync(id, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.ErrorCode, message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets a job category by MoHRE code.
    /// </summary>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(typeof(JobCategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code, CancellationToken ct)
    {
        var result = await _jobCategoryService.GetByCodeAsync(code, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.ErrorCode, message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets lightweight job category references for dropdowns.
    /// Only active categories, ordered by display order.
    /// </summary>
    [HttpGet("refs")]
    [ProducesResponseType(typeof(List<JobCategoryRefDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReferences(CancellationToken ct)
    {
        var result = await _jobCategoryService.GetReferencesAsync(ct);
        return Ok(result);
    }
}
