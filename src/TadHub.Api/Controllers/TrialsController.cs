using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trial.Contracts;
using Trial.Contracts.DTOs;
using Worker.Contracts;
using Client.Contracts;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace TadHub.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/trials")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class TrialsController : ControllerBase
{
    private readonly ITrialService _trialService;
    private readonly IWorkerService _workerService;
    private readonly IClientService _clientService;

    public TrialsController(
        ITrialService trialService,
        IWorkerService workerService,
        IClientService clientService)
    {
        _trialService = trialService;
        _workerService = workerService;
        _clientService = clientService;
    }

    [HttpGet]
    [HasPermission("trials.view")]
    [ProducesResponseType(typeof(PagedList<TrialListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _trialService.ListAsync(tenantId, qp, ct);
        result = await EnrichListWithParties(tenantId, result, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission("trials.view")]
    [ProducesResponseType(typeof(TrialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid tenantId,
        Guid id,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _trialService.GetByIdAsync(tenantId, id, qp, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = result.Value!;
        dto = await EnrichWithParties(tenantId, dto, ct);

        return Ok(dto);
    }

    [HttpPost]
    [HasPermission("trials.create")]
    [ProducesResponseType(typeof(TrialDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        Guid tenantId,
        [FromBody] CreateTrialRequest request,
        CancellationToken ct)
    {
        var result = await _trialService.CreateAsync(tenantId, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = result.Value!;
        dto = await EnrichWithParties(tenantId, dto, ct);

        return CreatedAtAction(nameof(GetById), new { tenantId, id = dto.Id }, dto);
    }

    [HttpPut("{id:guid}/complete")]
    [HasPermission("trials.manage")]
    [ProducesResponseType(typeof(TrialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(
        Guid tenantId,
        Guid id,
        [FromBody] CompleteTrialRequest request,
        CancellationToken ct)
    {
        var result = await _trialService.CompleteAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}/cancel")]
    [HasPermission("trials.manage")]
    [ProducesResponseType(typeof(TrialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(
        Guid tenantId,
        Guid id,
        [FromBody] CancelTrialRequest request,
        CancellationToken ct)
    {
        var result = await _trialService.CancelAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/status-history")]
    [HasPermission("trials.view")]
    [ProducesResponseType(typeof(List<TrialStatusHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatusHistory(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _trialService.GetStatusHistoryAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("trials.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _trialService.DeleteAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return NoContent();
    }

    #region BFF Enrichment

    private async Task<PagedList<TrialListDto>> EnrichListWithParties(
        Guid tenantId,
        PagedList<TrialListDto> pagedList,
        CancellationToken ct)
    {
        var workerIds = pagedList.Items.Select(t => t.WorkerId).Distinct().ToList();
        var clientIds = pagedList.Items.Select(t => t.ClientId).Distinct().ToList();

        var workerMap = new Dictionary<Guid, TrialWorkerRefDto>();
        var clientMap = new Dictionary<Guid, TrialClientRefDto>();

        foreach (var id in workerIds)
        {
            var result = await _workerService.GetByIdAsync(tenantId, id, ct: ct);
            if (!result.IsSuccess) continue;
            var w = result.Value!;
            workerMap[w.Id] = new TrialWorkerRefDto
            {
                Id = w.Id,
                FullNameEn = w.FullNameEn,
                FullNameAr = w.FullNameAr,
                WorkerCode = w.WorkerCode,
            };
        }

        foreach (var id in clientIds)
        {
            var result = await _clientService.GetByIdAsync(tenantId, id, ct);
            if (!result.IsSuccess) continue;
            var c = result.Value!;
            clientMap[c.Id] = new TrialClientRefDto
            {
                Id = c.Id,
                NameEn = c.NameEn,
                NameAr = c.NameAr,
            };
        }

        var enriched = pagedList.Items.Select(t => t with
        {
            Worker = workerMap.GetValueOrDefault(t.WorkerId),
            Client = clientMap.GetValueOrDefault(t.ClientId),
        }).ToList();

        return new PagedList<TrialListDto>(enriched, pagedList.TotalCount, pagedList.Page, pagedList.PageSize);
    }

    private async Task<TrialDto> EnrichWithParties(
        Guid tenantId,
        TrialDto dto,
        CancellationToken ct)
    {
        var workerResult = await _workerService.GetByIdAsync(tenantId, dto.WorkerId, ct: ct);
        if (workerResult.IsSuccess)
        {
            var w = workerResult.Value!;
            dto = dto with
            {
                Worker = new TrialWorkerRefDto
                {
                    Id = w.Id,
                    FullNameEn = w.FullNameEn,
                    FullNameAr = w.FullNameAr,
                    WorkerCode = w.WorkerCode,
                },
            };
        }

        var clientResult = await _clientService.GetByIdAsync(tenantId, dto.ClientId, ct);
        if (clientResult.IsSuccess)
        {
            var c = clientResult.Value!;
            dto = dto with
            {
                Client = new TrialClientRefDto
                {
                    Id = c.Id,
                    NameEn = c.NameEn,
                    NameAr = c.NameAr,
                },
            };
        }

        return dto;
    }

    #endregion

    #region Error Helpers

    private IActionResult MapResultError<T>(Result<T> result)
        => MapError(result.Error!, result.ErrorCode);

    private IActionResult MapResultError(Result result)
        => MapError(result.Error!, result.ErrorCode);

    private IActionResult MapError(string error, string? errorCode)
    {
        var path = HttpContext.Request.Path.Value;
        var (status, apiError) = errorCode switch
        {
            "NOT_FOUND" => (404, ApiError.NotFound(error, path)),
            "CONFLICT" => (409, ApiError.Conflict(error, path)),
            "FORBIDDEN" => (403, ApiError.Forbidden(error)),
            _ => (400, ApiError.BadRequest(error, path))
        };
        return new ObjectResult(apiError) { StatusCode = status, ContentTypes = { "application/problem+json" } };
    }

    #endregion
}
