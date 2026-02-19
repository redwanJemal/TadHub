using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TadHub.SharedKernel.Api;
using Worker.Contracts;
using Worker.Contracts.DTOs;

namespace Worker.Api.Controllers;

/// <summary>
/// Worker management endpoints.
/// </summary>
[ApiController]
[Route("api/v1/workers")]
[Authorize]
public class WorkersController : ControllerBase
{
    private readonly IWorkerService _workerService;

    public WorkersController(IWorkerService workerService)
    {
        _workerService = workerService;
    }

    /// <summary>
    /// Creates a new worker.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "workers.create")]
    [ProducesResponseType(typeof(WorkerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateWorkerRequest request, CancellationToken ct)
    {
        var result = await _workerService.CreateAsync(request, ct);

        if (!result.IsSuccess)
        {
            return result.ErrorCode == "DUPLICATE_PASSPORT"
                ? Conflict(new { error = result.ErrorCode, message = result.Error })
                : BadRequest(new { error = result.ErrorCode, message = result.Error });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Gets a worker by ID.
    /// </summary>
    /// <param name="id">Worker ID</param>
    /// <param name="include">Relations to include: skills, languages, media, jobCategory</param>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "workers.view")]
    [ProducesResponseType(typeof(WorkerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] string? include, CancellationToken ct)
    {
        var includes = IncludeResolver.Parse(include);
        var result = await _workerService.GetByIdAsync(id, includes, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.ErrorCode, message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Updates a worker's CV details.
    /// </summary>
    [HttpPatch("{id:guid}")]
    [Authorize(Policy = "workers.update")]
    [ProducesResponseType(typeof(WorkerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWorkerRequest request, CancellationToken ct)
    {
        var result = await _workerService.UpdateAsync(id, request, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.ErrorCode, message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Lists workers with filtering, sorting, and pagination.
    /// </summary>
    /// <remarks>
    /// Filters:
    /// - filter[status]=readyForMarket,inTraining (array)
    /// - filter[nationality]=philippines,indonesia (array)
    /// - filter[jobCategoryId]=...
    /// - filter[passportLocation]=withAgency
    /// - filter[isAvailableForFlexible]=true
    /// - filter[createdAt][gte]=2026-01-01
    /// 
    /// Sorting:
    /// - sort=createdAt:desc
    /// - sort=monthlyBaseSalary:asc
    /// 
    /// Includes:
    /// - include=skills,languages,media,jobCategory
    /// </remarks>
    [HttpGet]
    [Authorize(Policy = "workers.view")]
    [ProducesResponseType(typeof(PagedList<WorkerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] QueryParameters query, CancellationToken ct)
    {
        var result = await _workerService.ListAsync(query, ct);
        return Ok(result);
    }

    #region State Machine

    /// <summary>
    /// Transitions a worker to a new state.
    /// </summary>
    /// <remarks>
    /// Valid transitions depend on current state.
    /// Use GET /workers/{id}/valid-transitions to see available options.
    /// </remarks>
    [HttpPost("{id:guid}/transition")]
    [Authorize(Policy = "workers.manage")]
    [ProducesResponseType(typeof(WorkerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> TransitionState(Guid id, [FromBody] WorkerStateTransitionRequest request, CancellationToken ct)
    {
        var result = await _workerService.TransitionStateAsync(id, request, ct);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.ErrorCode, message = result.Error }),
                "INVALID_TRANSITION" or "PRECONDITION_FAILED" => 
                    Conflict(new { error = result.ErrorCode, message = result.Error }),
                _ => BadRequest(new { error = result.ErrorCode, message = result.Error })
            };
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets valid target states from current state.
    /// </summary>
    [HttpGet("{id:guid}/valid-transitions")]
    [Authorize(Policy = "workers.view")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetValidTransitions(Guid id, CancellationToken ct)
    {
        var result = await _workerService.GetValidTransitionsAsync(id, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.ErrorCode, message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets the state history for a worker.
    /// </summary>
    [HttpGet("{id:guid}/history")]
    [Authorize(Policy = "workers.view")]
    [ProducesResponseType(typeof(PagedList<WorkerStateHistoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStateHistory(Guid id, [FromQuery] QueryParameters query, CancellationToken ct)
    {
        var result = await _workerService.GetStateHistoryAsync(id, query, ct);
        return Ok(result);
    }

    #endregion

    #region Passport Custody

    /// <summary>
    /// Gets current passport custody information.
    /// </summary>
    [HttpGet("{id:guid}/passport-custody")]
    [Authorize(Policy = "workers.passport.view")]
    [ProducesResponseType(typeof(PassportCustodyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPassportCustody(Guid id, CancellationToken ct)
    {
        var result = await _workerService.GetPassportCustodyAsync(id, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.ErrorCode, message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets passport custody history (audit trail).
    /// </summary>
    [HttpGet("{id:guid}/passport-custody/history")]
    [Authorize(Policy = "workers.passport.view")]
    [ProducesResponseType(typeof(PagedList<PassportCustodyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPassportCustodyHistory(Guid id, [FromQuery] QueryParameters query, CancellationToken ct)
    {
        var result = await _workerService.GetPassportCustodyHistoryAsync(id, query, ct);
        return Ok(result);
    }

    /// <summary>
    /// Transfers passport custody.
    /// </summary>
    [HttpPost("{id:guid}/passport-transfer")]
    [Authorize(Policy = "workers.passport.manage")]
    [ProducesResponseType(typeof(PassportCustodyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransferPassport(Guid id, [FromBody] TransferPassportRequest request, CancellationToken ct)
    {
        var result = await _workerService.TransferPassportAsync(id, request, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.ErrorCode, message = result.Error });

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    #endregion

    #region Skills & Languages

    /// <summary>
    /// Adds or updates a skill for a worker.
    /// </summary>
    [HttpPut("{id:guid}/skills/{skillName}")]
    [Authorize(Policy = "workers.update")]
    [ProducesResponseType(typeof(WorkerSkillDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpsertSkill(Guid id, string skillName, [FromBody] int rating, CancellationToken ct)
    {
        var result = await _workerService.UpsertSkillAsync(id, new CreateWorkerSkillRequest
        {
            SkillName = skillName,
            Rating = rating
        }, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.ErrorCode, message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Removes a skill from a worker.
    /// </summary>
    [HttpDelete("{id:guid}/skills/{skillName}")]
    [Authorize(Policy = "workers.update")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveSkill(Guid id, string skillName, CancellationToken ct)
    {
        var result = await _workerService.RemoveSkillAsync(id, skillName, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.ErrorCode, message = result.Error });

        return NoContent();
    }

    /// <summary>
    /// Adds or updates a language for a worker.
    /// </summary>
    [HttpPut("{id:guid}/languages/{language}")]
    [Authorize(Policy = "workers.update")]
    [ProducesResponseType(typeof(WorkerLanguageDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpsertLanguage(Guid id, string language, [FromBody] string proficiency, CancellationToken ct)
    {
        var result = await _workerService.UpsertLanguageAsync(id, new CreateWorkerLanguageRequest
        {
            Language = language,
            Proficiency = proficiency
        }, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.ErrorCode, message = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Removes a language from a worker.
    /// </summary>
    [HttpDelete("{id:guid}/languages/{language}")]
    [Authorize(Policy = "workers.update")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveLanguage(Guid id, string language, CancellationToken ct)
    {
        var result = await _workerService.RemoveLanguageAsync(id, language, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.ErrorCode, message = result.Error });

        return NoContent();
    }

    #endregion

    #region Media

    /// <summary>
    /// Adds media to a worker.
    /// </summary>
    [HttpPost("{id:guid}/media")]
    [Authorize(Policy = "workers.update")]
    [ProducesResponseType(typeof(WorkerMediaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddMedia(Guid id, [FromBody] AddWorkerMediaRequest request, CancellationToken ct)
    {
        var result = await _workerService.AddMediaAsync(id, request, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.ErrorCode, message = result.Error });

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    /// <summary>
    /// Removes media from a worker.
    /// </summary>
    [HttpDelete("{id:guid}/media/{mediaId:guid}")]
    [Authorize(Policy = "workers.update")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMedia(Guid id, Guid mediaId, CancellationToken ct)
    {
        var result = await _workerService.RemoveMediaAsync(id, mediaId, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.ErrorCode, message = result.Error });

        return NoContent();
    }

    /// <summary>
    /// Sets a media item as primary.
    /// </summary>
    [HttpPost("{id:guid}/media/{mediaId:guid}/set-primary")]
    [Authorize(Policy = "workers.update")]
    [ProducesResponseType(typeof(WorkerMediaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPrimaryMedia(Guid id, Guid mediaId, CancellationToken ct)
    {
        var result = await _workerService.SetPrimaryMediaAsync(id, mediaId, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.ErrorCode, message = result.Error });

        return Ok(result.Value);
    }

    #endregion
}
