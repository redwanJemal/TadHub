using SaasKit.SharedKernel.Entities;

namespace Subscription.Core.Entities;

/// <summary>
/// Tracks Stripe checkout sessions for subscription changes.
/// </summary>
public class CheckoutSession : TenantScopedEntity
{
    /// <summary>
    /// Stripe Checkout Session ID.
    /// </summary>
    public string StripeSessionId { get; set; } = string.Empty;

    /// <summary>
    /// Target plan for this checkout.
    /// </summary>
    public Guid PlanId { get; set; }
    public Plan Plan { get; set; } = null!;

    /// <summary>
    /// Target price for this checkout.
    /// </summary>
    public Guid PlanPriceId { get; set; }
    public PlanPrice PlanPrice { get; set; } = null!;

    /// <summary>
    /// Session status: pending, completed, expired, canceled.
    /// </summary>
    public string Status { get; set; } = "pending";

    /// <summary>
    /// Checkout URL.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Session expiration time.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    /// When the session was completed.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// User who initiated the checkout.
    /// </summary>
    public Guid InitiatedByUserId { get; set; }
}
