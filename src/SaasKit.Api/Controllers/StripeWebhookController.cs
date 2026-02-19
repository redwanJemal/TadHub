using Microsoft.AspNetCore.Mvc;
using Subscription.Contracts;

namespace SaasKit.Api.Controllers;

/// <summary>
/// Stripe webhook endpoint.
/// </summary>
[ApiController]
[Route("api/v1/webhooks")]
public class StripeWebhookController : ControllerBase
{
    private readonly ISubscriptionService _subscriptionService;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(
        ISubscriptionService subscriptionService,
        ILogger<StripeWebhookController> logger)
    {
        _subscriptionService = subscriptionService;
        _logger = logger;
    }

    /// <summary>
    /// Handles Stripe webhook events.
    /// This endpoint is called by Stripe and should NOT require authentication.
    /// The webhook signature is validated in the service.
    /// </summary>
    [HttpPost("stripe")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> HandleStripeWebhook(CancellationToken ct)
    {
        var signature = Request.Headers["Stripe-Signature"].FirstOrDefault();
        if (string.IsNullOrEmpty(signature))
        {
            _logger.LogWarning("Stripe webhook received without signature");
            return BadRequest("Missing Stripe-Signature header");
        }

        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(ct);

        try
        {
            await _subscriptionService.HandleStripeWebhookAsync(payload, signature, ct);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process Stripe webhook");
            return BadRequest("Webhook processing failed");
        }
    }
}
