using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Visa.Contracts;
using Visa.Contracts.DTOs;
using Worker.Contracts;
using Client.Contracts;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace TadHub.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/visa-applications")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class VisaApplicationsController : ControllerBase
{
    private readonly IVisaApplicationService _visaService;
    private readonly IWorkerService _workerService;
    private readonly IClientService _clientService;

    public VisaApplicationsController(
        IVisaApplicationService visaService,
        IWorkerService workerService,
        IClientService clientService)
    {
        _visaService = visaService;
        _workerService = workerService;
        _clientService = clientService;
    }

    [HttpGet]
    [HasPermission("visas.view")]
    [ProducesResponseType(typeof(PagedList<VisaApplicationListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _visaService.ListAsync(tenantId, qp, ct);
        result = await EnrichListWithParties(tenantId, result, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [HasPermission("visas.view")]
    [ProducesResponseType(typeof(VisaApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid tenantId,
        Guid id,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _visaService.GetByIdAsync(tenantId, id, qp, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = result.Value!;
        dto = await EnrichWithParties(tenantId, dto, ct);

        return Ok(dto);
    }

    [HttpPost]
    [HasPermission("visas.create")]
    [ProducesResponseType(typeof(VisaApplicationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        Guid tenantId,
        [FromBody] CreateVisaApplicationRequest request,
        CancellationToken ct)
    {
        var result = await _visaService.CreateAsync(tenantId, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        var dto = result.Value!;
        dto = await EnrichWithParties(tenantId, dto, ct);

        return CreatedAtAction(nameof(GetById), new { tenantId, id = dto.Id }, dto);
    }

    [HttpPatch("{id:guid}")]
    [HasPermission("visas.manage")]
    [ProducesResponseType(typeof(VisaApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid tenantId,
        Guid id,
        [FromBody] UpdateVisaApplicationRequest request,
        CancellationToken ct)
    {
        var result = await _visaService.UpdateAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/transition")]
    [HasPermission("visas.manage")]
    [ProducesResponseType(typeof(VisaApplicationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransitionStatus(
        Guid tenantId,
        Guid id,
        [FromBody] TransitionVisaStatusRequest request,
        CancellationToken ct)
    {
        var result = await _visaService.TransitionStatusAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpGet("{id:guid}/status-history")]
    [HasPermission("visas.view")]
    [ProducesResponseType(typeof(List<VisaApplicationStatusHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatusHistory(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _visaService.GetStatusHistoryAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/documents")]
    [HasPermission("visas.manage")]
    [ProducesResponseType(typeof(VisaApplicationDocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadDocument(
        Guid tenantId,
        Guid id,
        [FromBody] UploadVisaDocumentRequest request,
        CancellationToken ct)
    {
        var result = await _visaService.UploadDocumentAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return StatusCode(201, result.Value);
    }

    [HttpDelete("{id:guid}")]
    [HasPermission("visas.delete")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _visaService.DeleteAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return NoContent();
    }

    [HttpGet("by-worker/{workerId:guid}")]
    [HasPermission("visas.view")]
    [ProducesResponseType(typeof(List<VisaApplicationListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByWorker(
        Guid tenantId,
        Guid workerId,
        CancellationToken ct)
    {
        var result = await _visaService.GetByWorkerAsync(tenantId, workerId, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    #region BFF Enrichment

    private async Task<PagedList<VisaApplicationListDto>> EnrichListWithParties(
        Guid tenantId,
        PagedList<VisaApplicationListDto> pagedList,
        CancellationToken ct)
    {
        var workerIds = pagedList.Items.Select(p => p.WorkerId).Distinct().ToList();
        var clientIds = pagedList.Items.Select(p => p.ClientId).Distinct().ToList();

        var workerMap = new Dictionary<Guid, VisaWorkerRefDto>();
        var clientMap = new Dictionary<Guid, VisaClientRefDto>();

        // Fetch sequentially — DbContext is not thread-safe
        foreach (var id in workerIds)
        {
            var result = await _workerService.GetByIdAsync(tenantId, id, ct: ct);
            if (!result.IsSuccess) continue;
            var w = result.Value!;
            workerMap[w.Id] = new VisaWorkerRefDto
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
            clientMap[c.Id] = new VisaClientRefDto
            {
                Id = c.Id,
                NameEn = c.NameEn,
                NameAr = c.NameAr,
            };
        }

        var enriched = pagedList.Items.Select(p => p with
        {
            Worker = workerMap.GetValueOrDefault(p.WorkerId),
            Client = clientMap.GetValueOrDefault(p.ClientId),
        }).ToList();

        return new PagedList<VisaApplicationListDto>(enriched, pagedList.TotalCount, pagedList.Page, pagedList.PageSize);
    }

    private async Task<VisaApplicationDto> EnrichWithParties(
        Guid tenantId,
        VisaApplicationDto dto,
        CancellationToken ct)
    {
        // Fetch sequentially — DbContext is not thread-safe
        var workerResult = await _workerService.GetByIdAsync(tenantId, dto.WorkerId, ct: ct);
        if (workerResult.IsSuccess)
        {
            var w = workerResult.Value!;
            dto = dto with
            {
                Worker = new VisaWorkerRefDto
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
                Client = new VisaClientRefDto
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
