using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Runaway.Contracts;
using Runaway.Contracts.DTOs;
using Worker.Contracts;
using Client.Contracts;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace TadHub.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/runaway-cases")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class RunawayCasesController : ControllerBase
{
    private readonly IRunawayService _runawayService;
    private readonly IWorkerService _workerService;
    private readonly IClientService _clientService;

    public RunawayCasesController(
        IRunawayService runawayService,
        IWorkerService workerService,
        IClientService clientService)
    {
        _runawayService = runawayService;
        _workerService = workerService;
        _clientService = clientService;
    }

    [HttpGet]
    [HasPermission("runaways.view")]
    [ProducesResponseType(typeof(PagedList<RunawayCaseListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _runawayService.ListAsync(tenantId, qp, ct);
        result = await EnrichListWithParties(tenantId, result, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission("runaways.view")]
    [ProducesResponseType(typeof(RunawayCaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid tenantId,
        Guid id,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _runawayService.GetByIdAsync(tenantId, id, qp, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = result.Value!;
        dto = await EnrichWithParties(tenantId, dto, ct);

        return Ok(dto);
    }

    [HttpPost]
    [HasPermission("runaways.report")]
    [ProducesResponseType(typeof(RunawayCaseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Report(
        Guid tenantId,
        [FromBody] ReportRunawayCaseRequest request,
        CancellationToken ct)
    {
        var result = await _runawayService.ReportAsync(tenantId, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = result.Value!;
        dto = await EnrichWithParties(tenantId, dto, ct);

        return CreatedAtAction(nameof(GetById), new { tenantId, id = dto.Id }, dto);
    }

    [HttpPatch("{id:guid}")]
    [HasPermission("runaways.manage")]
    [ProducesResponseType(typeof(RunawayCaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid tenantId,
        Guid id,
        [FromBody] UpdateRunawayCaseRequest request,
        CancellationToken ct)
    {
        var result = await _runawayService.UpdateAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}/confirm")]
    [HasPermission("runaways.manage")]
    [ProducesResponseType(typeof(RunawayCaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirm(
        Guid tenantId,
        Guid id,
        [FromBody] ConfirmRunawayCaseRequest request,
        CancellationToken ct)
    {
        var result = await _runawayService.ConfirmAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}/settle")]
    [HasPermission("runaways.settle")]
    [ProducesResponseType(typeof(RunawayCaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Settle(
        Guid tenantId,
        Guid id,
        [FromBody] SettleRunawayCaseRequest request,
        CancellationToken ct)
    {
        var result = await _runawayService.SettleAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}/close")]
    [HasPermission("runaways.manage")]
    [ProducesResponseType(typeof(RunawayCaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Close(
        Guid tenantId,
        Guid id,
        [FromBody] CloseRunawayCaseRequest request,
        CancellationToken ct)
    {
        var result = await _runawayService.CloseAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/expenses")]
    [HasPermission("runaways.manage")]
    [ProducesResponseType(typeof(RunawayExpenseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddExpense(
        Guid tenantId,
        Guid id,
        [FromBody] CreateRunawayExpenseRequest request,
        CancellationToken ct)
    {
        var result = await _runawayService.AddExpenseAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpGet("{id:guid}/status-history")]
    [HasPermission("runaways.view")]
    [ProducesResponseType(typeof(List<RunawayCaseStatusHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatusHistory(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _runawayService.GetStatusHistoryAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("runaways.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _runawayService.DeleteAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return NoContent();
    }

    #region BFF Enrichment

    private async Task<PagedList<RunawayCaseListDto>> EnrichListWithParties(
        Guid tenantId,
        PagedList<RunawayCaseListDto> pagedList,
        CancellationToken ct)
    {
        var workerIds = pagedList.Items.Select(x => x.WorkerId).Distinct().ToList();
        var clientIds = pagedList.Items.Select(x => x.ClientId).Distinct().ToList();

        var workerMap = new Dictionary<Guid, RunawayWorkerRefDto>();
        var clientMap = new Dictionary<Guid, RunawayClientRefDto>();

        foreach (var wid in workerIds)
        {
            var result = await _workerService.GetByIdAsync(tenantId, wid, ct: ct);
            if (!result.IsSuccess) continue;
            var w = result.Value!;
            workerMap[w.Id] = new RunawayWorkerRefDto
            {
                Id = w.Id,
                FullNameEn = w.FullNameEn,
                FullNameAr = w.FullNameAr,
                WorkerCode = w.WorkerCode,
            };
        }

        foreach (var cid in clientIds)
        {
            var result = await _clientService.GetByIdAsync(tenantId, cid, ct);
            if (!result.IsSuccess) continue;
            var c = result.Value!;
            clientMap[c.Id] = new RunawayClientRefDto
            {
                Id = c.Id,
                NameEn = c.NameEn,
                NameAr = c.NameAr,
            };
        }

        var enriched = pagedList.Items.Select(x => x with
        {
            Worker = workerMap.GetValueOrDefault(x.WorkerId),
            Client = clientMap.GetValueOrDefault(x.ClientId),
        }).ToList();

        return new PagedList<RunawayCaseListDto>(enriched, pagedList.TotalCount, pagedList.Page, pagedList.PageSize);
    }

    private async Task<RunawayCaseDto> EnrichWithParties(
        Guid tenantId,
        RunawayCaseDto dto,
        CancellationToken ct)
    {
        var workerResult = await _workerService.GetByIdAsync(tenantId, dto.WorkerId, ct: ct);
        if (workerResult.IsSuccess)
        {
            var w = workerResult.Value!;
            dto = dto with
            {
                Worker = new RunawayWorkerRefDto
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
                Client = new RunawayClientRefDto
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
