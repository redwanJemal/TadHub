using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Financial.Contracts;
using Financial.Contracts.DTOs;
using Client.Contracts;
using Worker.Contracts;
using Contract.Contracts;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace TadHub.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/financial-reports")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class FinancialReportsController : ControllerBase
{
    private readonly IFinancialReportService _reportService;
    private readonly IClientService _clientService;
    private readonly IWorkerService _workerService;
    private readonly IContractService _contractService;

    public FinancialReportsController(
        IFinancialReportService reportService,
        IClientService clientService,
        IWorkerService workerService,
        IContractService contractService)
    {
        _reportService = reportService;
        _clientService = clientService;
        _workerService = workerService;
        _contractService = contractService;
    }

    [HttpGet("margin")]
    [HasPermission("financial_reports.view")]
    [ProducesResponseType(typeof(MarginReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMarginReport(
        Guid tenantId,
        CancellationToken ct)
    {
        var result = await _reportService.GetMarginReportAsync(tenantId, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        var report = await EnrichMarginReport(tenantId, result.Value!, ct);
        return Ok(report);
    }

    [HttpGet("revenue-breakdown")]
    [HasPermission("financial_reports.view")]
    [ProducesResponseType(typeof(RevenueBreakdownDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRevenueBreakdown(
        Guid tenantId,
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken ct)
    {
        var result = await _reportService.GetRevenueBreakdownAsync(tenantId, from, to, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpPost("x-report")]
    [HasPermission("financial_reports.manage")]
    [ProducesResponseType(typeof(CashReconciliationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> GenerateXReport(
        Guid tenantId,
        [FromQuery] DateOnly? reportDate,
        CancellationToken ct)
    {
        var result = await _reportService.GenerateXReportAsync(tenantId, reportDate, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpPost("x-report/{id:guid}/close")]
    [HasPermission("financial_reports.manage")]
    [ProducesResponseType(typeof(CashReconciliationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CloseXReport(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _reportService.CloseXReportAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    [HttpGet("x-reports")]
    [HasPermission("financial_reports.view")]
    [ProducesResponseType(typeof(PagedList<CashReconciliationListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListXReports(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _reportService.ListXReportsAsync(tenantId, qp, ct);
        return Ok(result);
    }

    #region BFF Enrichment

    private async Task<MarginReportDto> EnrichMarginReport(
        Guid tenantId, MarginReportDto report, CancellationToken ct)
    {
        var clientIds = report.Lines.Where(l => l.ClientId.HasValue).Select(l => l.ClientId!.Value).Distinct().ToList();
        var workerIds = report.Lines.Where(l => l.WorkerId.HasValue).Select(l => l.WorkerId!.Value).Distinct().ToList();
        var contractIds = report.Lines.Where(l => l.ContractId.HasValue).Select(l => l.ContractId!.Value).Distinct().ToList();

        var clientMap = new Dictionary<Guid, InvoiceClientRef>();
        if (clientIds.Count > 0)
        {
            var clients = await _clientService.ListAsync(tenantId, new QueryParameters { PageSize = clientIds.Count }, ct);
            foreach (var c in clients.Items.Where(c => clientIds.Contains(c.Id)))
                clientMap[c.Id] = new InvoiceClientRef { Id = c.Id, NameEn = c.NameEn, NameAr = c.NameAr };
        }

        var workerMap = new Dictionary<Guid, InvoiceWorkerRef>();
        if (workerIds.Count > 0)
        {
            var workers = await _workerService.ListAsync(tenantId, new QueryParameters { PageSize = workerIds.Count }, ct);
            foreach (var w in workers.Items.Where(w => workerIds.Contains(w.Id)))
                workerMap[w.Id] = new InvoiceWorkerRef { Id = w.Id, FullNameEn = w.FullNameEn, FullNameAr = w.FullNameAr, WorkerCode = w.WorkerCode };
        }

        var contractMap = new Dictionary<Guid, InvoiceContractRef>();
        if (contractIds.Count > 0)
        {
            var contracts = await _contractService.ListAsync(tenantId, new QueryParameters { PageSize = contractIds.Count }, ct);
            foreach (var c in contracts.Items.Where(c => contractIds.Contains(c.Id)))
                contractMap[c.Id] = new InvoiceContractRef { Id = c.Id, ContractCode = c.ContractCode };
        }

        var enrichedLines = report.Lines.Select(l => l with
        {
            Client = l.ClientId.HasValue ? clientMap.GetValueOrDefault(l.ClientId.Value) : null,
            Worker = l.WorkerId.HasValue ? workerMap.GetValueOrDefault(l.WorkerId.Value) : null,
            Contract = l.ContractId.HasValue ? contractMap.GetValueOrDefault(l.ContractId.Value) : null,
        }).ToList();

        return report with { Lines = enrichedLines };
    }

    #endregion

    #region Error Helpers

    private IActionResult MapResultError<T>(Result<T> result)
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
