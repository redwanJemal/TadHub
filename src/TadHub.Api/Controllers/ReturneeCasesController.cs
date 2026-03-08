using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Returnee.Contracts;
using Returnee.Contracts.DTOs;
using Worker.Contracts;
using Client.Contracts;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace TadHub.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/returnee-cases")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class ReturneeCasesController : ControllerBase
{
    private readonly IReturneeService _returneeService;
    private readonly IWorkerService _workerService;
    private readonly IClientService _clientService;

    public ReturneeCasesController(
        IReturneeService returneeService,
        IWorkerService workerService,
        IClientService clientService)
    {
        _returneeService = returneeService;
        _workerService = workerService;
        _clientService = clientService;
    }

    [HttpGet]
    [HasPermission("returnees.view")]
    [ProducesResponseType(typeof(PagedList<ReturneeCaseListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _returneeService.ListAsync(tenantId, qp, ct);
        result = await EnrichListWithParties(tenantId, result, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission("returnees.view")]
    [ProducesResponseType(typeof(ReturneeCaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid tenantId,
        Guid id,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _returneeService.GetByIdAsync(tenantId, id, qp, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = result.Value!;
        dto = await EnrichWithParties(tenantId, dto, ct);

        return Ok(dto);
    }

    [HttpPost]
    [HasPermission("returnees.create")]
    [ProducesResponseType(typeof(ReturneeCaseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        Guid tenantId,
        [FromBody] CreateReturneeCaseRequest request,
        CancellationToken ct)
    {
        var result = await _returneeService.CreateAsync(tenantId, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = result.Value!;
        dto = await EnrichWithParties(tenantId, dto, ct);

        return CreatedAtAction(nameof(GetById), new { tenantId, id = dto.Id }, dto);
    }

    [HttpPut("{id:guid}/approve")]
    [HasPermission("returnees.manage")]
    [ProducesResponseType(typeof(ReturneeCaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(
        Guid tenantId,
        Guid id,
        [FromBody] ApproveReturneeCaseRequest request,
        CancellationToken ct)
    {
        var result = await _returneeService.ApproveAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}/reject")]
    [HasPermission("returnees.manage")]
    [ProducesResponseType(typeof(ReturneeCaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(
        Guid tenantId,
        Guid id,
        [FromBody] RejectReturneeCaseRequest request,
        CancellationToken ct)
    {
        var result = await _returneeService.RejectAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPut("{id:guid}/settle")]
    [HasPermission("returnees.settle")]
    [ProducesResponseType(typeof(ReturneeCaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Settle(
        Guid tenantId,
        Guid id,
        [FromBody] SettleReturneeCaseRequest request,
        CancellationToken ct)
    {
        var result = await _returneeService.SettleAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/expenses")]
    [HasPermission("returnees.manage")]
    [ProducesResponseType(typeof(ReturneeExpenseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddExpense(
        Guid tenantId,
        Guid id,
        [FromBody] CreateReturneeExpenseRequest request,
        CancellationToken ct)
    {
        var result = await _returneeService.AddExpenseAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpGet("{id:guid}/refund-calculation")]
    [HasPermission("returnees.view")]
    [ProducesResponseType(typeof(RefundCalculationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRefundCalculation(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _returneeService.CalculateRefundAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/status-history")]
    [HasPermission("returnees.view")]
    [ProducesResponseType(typeof(List<ReturneeCaseStatusHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatusHistory(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _returneeService.GetStatusHistoryAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("returnees.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _returneeService.DeleteAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return NoContent();
    }

    #region BFF Enrichment

    private async Task<PagedList<ReturneeCaseListDto>> EnrichListWithParties(
        Guid tenantId,
        PagedList<ReturneeCaseListDto> pagedList,
        CancellationToken ct)
    {
        var workerIds = pagedList.Items.Select(x => x.WorkerId).Distinct().ToList();
        var clientIds = pagedList.Items.Select(x => x.ClientId).Distinct().ToList();

        var workerMap = new Dictionary<Guid, ReturneeWorkerRefDto>();
        var clientMap = new Dictionary<Guid, ReturneeClientRefDto>();

        foreach (var wid in workerIds)
        {
            var result = await _workerService.GetByIdAsync(tenantId, wid, ct: ct);
            if (!result.IsSuccess) continue;
            var w = result.Value!;
            workerMap[w.Id] = new ReturneeWorkerRefDto
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
            clientMap[c.Id] = new ReturneeClientRefDto
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

        return new PagedList<ReturneeCaseListDto>(enriched, pagedList.TotalCount, pagedList.Page, pagedList.PageSize);
    }

    private async Task<ReturneeCaseDto> EnrichWithParties(
        Guid tenantId,
        ReturneeCaseDto dto,
        CancellationToken ct)
    {
        var workerResult = await _workerService.GetByIdAsync(tenantId, dto.WorkerId, ct: ct);
        if (workerResult.IsSuccess)
        {
            var w = workerResult.Value!;
            dto = dto with
            {
                Worker = new ReturneeWorkerRefDto
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
                Client = new ReturneeClientRefDto
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
