using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaasKit.Api.Filters;
using SaasKit.Infrastructure.Auth;
using SaasKit.SharedKernel.Api;
using Subscription.Contracts;
using Subscription.Contracts.DTOs;

namespace SaasKit.Api.Controllers;

/// <summary>
/// Credit balance and ledger endpoints.
/// </summary>
[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/credits")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class CreditsController : ControllerBase
{
    private readonly ICreditService _creditService;

    public CreditsController(ICreditService creditService)
    {
        _creditService = creditService;
    }

    /// <summary>
    /// Gets the current credit balance for a tenant.
    /// </summary>
    [HttpGet("balance")]
    [HasPermission("billing.view")]
    [ProducesResponseType(typeof(CreditBalanceDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBalance(Guid tenantId, CancellationToken ct)
    {
        var result = await _creditService.GetBalanceAsync(tenantId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets credit transaction history.
    /// Supports: filter[type]=purchase, filter[type]=spend, filter[createdAt][gte]=2026-01-01, sort=-createdAt
    /// </summary>
    [HttpGet("history")]
    [HasPermission("billing.view")]
    [ProducesResponseType(typeof(IEnumerable<CreditDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _creditService.GetHistoryAsync(tenantId, qp, ct);
        return Ok(result);
    }

    /// <summary>
    /// Adds credits to a tenant (admin use).
    /// </summary>
    [HttpPost("add")]
    [HasPermission("billing.manage")]
    [ProducesResponseType(typeof(CreditDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddCredits(
        Guid tenantId,
        [FromBody] AddCreditsRequest request,
        CancellationToken ct)
    {
        var result = await _creditService.AddCreditsAsync(tenantId, request, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Spends credits from a tenant (for manual deductions).
    /// </summary>
    [HttpPost("spend")]
    [HasPermission("billing.manage")]
    [ProducesResponseType(typeof(CreditDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SpendCredits(
        Guid tenantId,
        [FromBody] SpendCreditsRequest request,
        CancellationToken ct)
    {
        var result = await _creditService.SpendCreditsAsync(tenantId, request, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value);
    }
}
