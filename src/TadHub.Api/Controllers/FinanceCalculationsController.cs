using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Financial.Contracts;
using Financial.Contracts.DTOs;
using Financial.Contracts.Settings;
using Tenancy.Contracts;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace TadHub.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/finance")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class FinanceCalculationsController : ControllerBase
{
    private readonly IRefundCalculationService _refundCalcService;
    private readonly ITenantService _tenantService;

    public FinanceCalculationsController(
        IRefundCalculationService refundCalcService,
        ITenantService tenantService)
    {
        _refundCalcService = refundCalcService;
        _tenantService = tenantService;
    }

    /// <summary>
    /// Preview refund calculation for a contract.
    /// </summary>
    [HttpGet("refund-calculation")]
    [HasPermission("invoices.view")]
    [ProducesResponseType(typeof(RefundCalculationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CalculateRefund(
        Guid tenantId,
        [FromQuery] Guid contractId,
        [FromQuery] DateOnly returnDate,
        CancellationToken ct)
    {
        var settings = await GetFinancialSettingsAsync(tenantId, ct);

        var result = await _refundCalcService.CalculateRefundAsync(
            tenantId,
            contractId,
            returnDate,
            settings.RefundSettings.DefaultContractMonths,
            settings.RefundSettings.PartialMonthCalculation,
            ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    /// <summary>
    /// Trigger commission calculation for a placement.
    /// </summary>
    [HttpPost("commission/calculate")]
    [HasPermission("supplier_payments.create")]
    [ProducesResponseType(typeof(CommissionCalculationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CalculateCommission(
        Guid tenantId,
        [FromQuery] Guid placementId,
        CancellationToken ct)
    {
        var settings = await GetFinancialSettingsAsync(tenantId, ct);

        var result = await _refundCalcService.CalculateCommissionAsync(
            tenantId,
            placementId,
            settings.CommissionSettings.CalculationType,
            settings.CommissionSettings.FixedAmount,
            settings.CommissionSettings.Percentage,
            ct);

        if (!result.IsSuccess)
            return MapResultError(result);

        return Ok(result.Value);
    }

    private async Task<TenantFinancialSettings> GetFinancialSettingsAsync(Guid tenantId, CancellationToken ct)
    {
        var settingsResult = await _tenantService.GetSettingsJsonAsync(tenantId, ct);
        if (settingsResult.IsSuccess && !string.IsNullOrWhiteSpace(settingsResult.Value))
        {
            var root = JsonNode.Parse(settingsResult.Value);
            var financialNode = root?["financial"];
            if (financialNode is not null)
            {
                return financialNode.Deserialize<TenantFinancialSettings>() ?? new TenantFinancialSettings();
            }
        }
        return new TenantFinancialSettings();
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
