using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Financial.Contracts;
using Financial.Contracts.DTOs;
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

    public FinancialReportsController(IFinancialReportService reportService)
    {
        _reportService = reportService;
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

        return Ok(result.Value);
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
