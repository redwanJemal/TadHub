using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reporting.Contracts;
using Reporting.Contracts.DTOs;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace TadHub.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/reports")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    // ── Workforce Reports ──

    [HttpGet("inventory")]
    [HasPermission("reports.view")]
    [ProducesResponseType(typeof(PagedList<InventoryReportItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventoryReport(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _reportService.GetInventoryReportAsync(tenantId, qp, ct);
        return Ok(result);
    }

    [HttpGet("deployed")]
    [HasPermission("reports.view")]
    [ProducesResponseType(typeof(PagedList<DeployedReportItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeployedReport(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _reportService.GetDeployedReportAsync(tenantId, qp, ct);
        return Ok(result);
    }

    [HttpGet("returnees")]
    [HasPermission("reports.view")]
    [ProducesResponseType(typeof(PagedList<ReturneeReportItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReturneeReport(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _reportService.GetReturneeReportAsync(tenantId, qp, ct);
        return Ok(result);
    }

    [HttpGet("runaways")]
    [HasPermission("reports.view")]
    [ProducesResponseType(typeof(PagedList<RunawayReportItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRunawayReport(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _reportService.GetRunawayReportAsync(tenantId, qp, ct);
        return Ok(result);
    }

    // ── Operational Reports ──

    [HttpGet("arrivals")]
    [HasPermission("reports.view")]
    [ProducesResponseType(typeof(PagedList<ArrivalReportItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetArrivalsReport(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _reportService.GetArrivalsReportAsync(tenantId, qp, ct);
        return Ok(result);
    }

    [HttpGet("accommodation-daily")]
    [HasPermission("reports.view")]
    [ProducesResponseType(typeof(PagedList<AccommodationDailyItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccommodationDailyList(
        Guid tenantId,
        [FromQuery] DateOnly? date,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var reportDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await _reportService.GetAccommodationDailyListAsync(tenantId, reportDate, qp, ct);
        return Ok(result);
    }

    [HttpGet("deployment-pipeline")]
    [HasPermission("reports.view")]
    [ProducesResponseType(typeof(List<DeploymentPipelineItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeploymentPipeline(
        Guid tenantId,
        CancellationToken ct)
    {
        var result = await _reportService.GetDeploymentPipelineAsync(tenantId, ct);
        return Ok(result);
    }

    // ── Finance Reports (Extensions) ──

    [HttpGet("supplier-commissions")]
    [HasPermission("reports.view")]
    [ProducesResponseType(typeof(PagedList<SupplierCommissionItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSupplierCommissionReport(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _reportService.GetSupplierCommissionReportAsync(tenantId, qp, ct);
        return Ok(result);
    }

    [HttpGet("refunds")]
    [HasPermission("reports.view")]
    [ProducesResponseType(typeof(PagedList<RefundReportItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRefundReport(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _reportService.GetRefundReportAsync(tenantId, qp, ct);
        return Ok(result);
    }

    [HttpGet("cost-per-maid")]
    [HasPermission("reports.view")]
    [ProducesResponseType(typeof(PagedList<CostPerMaidItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCostPerMaidReport(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _reportService.GetCostPerMaidReportAsync(tenantId, qp, ct);
        return Ok(result);
    }
}
