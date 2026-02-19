using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaasKit.Api.Filters;
using SaasKit.Infrastructure.Auth;
using Subscription.Contracts;
using Subscription.Contracts.DTOs;

namespace SaasKit.Api.Controllers;

/// <summary>
/// Tenant subscription management endpoints.
/// </summary>
[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/subscription")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class SubscriptionsController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;

    public SubscriptionsController(ISubscriptionService subscriptionService)
    {
        _subscriptionService = subscriptionService;
    }

    /// <summary>
    /// Gets the current subscription for a tenant.
    /// </summary>
    [HttpGet]
    [HasPermission("billing.view")]
    [ProducesResponseType(typeof(TenantSubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSubscription(Guid tenantId, CancellationToken ct)
    {
        var result = await _subscriptionService.GetSubscriptionAsync(tenantId, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a checkout session for subscription upgrade/change.
    /// </summary>
    [HttpPost("checkout")]
    [HasPermission("billing.manage")]
    [ProducesResponseType(typeof(CheckoutSessionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateCheckout(
        Guid tenantId,
        [FromBody] CreateCheckoutRequest request,
        CancellationToken ct)
    {
        var result = await _subscriptionService.CreateCheckoutSessionAsync(tenantId, request, ct);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
                return NotFound(new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Cancels the subscription (at period end or immediately).
    /// </summary>
    [HttpPost("cancel")]
    [HasPermission("billing.manage")]
    [ProducesResponseType(typeof(TenantSubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelSubscription(
        Guid tenantId,
        [FromBody] CancelSubscriptionRequest request,
        CancellationToken ct)
    {
        var result = await _subscriptionService.CancelSubscriptionAsync(tenantId, request, ct);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
                return NotFound(new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Resumes a subscription scheduled for cancellation.
    /// </summary>
    [HttpPost("resume")]
    [HasPermission("billing.manage")]
    [ProducesResponseType(typeof(TenantSubscriptionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResumeSubscription(Guid tenantId, CancellationToken ct)
    {
        var result = await _subscriptionService.ResumeSubscriptionAsync(tenantId, ct);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
                return NotFound(new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }

        return Ok(result.Value);
    }
}
