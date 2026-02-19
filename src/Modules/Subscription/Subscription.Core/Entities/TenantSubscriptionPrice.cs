using TadHub.SharedKernel.Entities;

namespace Subscription.Core.Entities;

/// <summary>
/// Represents a price component of a subscription product.
/// </summary>
public class TenantSubscriptionPrice : TenantScopedEntity
{
    /// <summary>
    /// Parent subscription product.
    /// </summary>
    public Guid TenantSubscriptionProductId { get; set; }
    public TenantSubscriptionProduct TenantSubscriptionProduct { get; set; } = null!;

    /// <summary>
    /// Stripe Price ID.
    /// </summary>
    public string? StripePriceId { get; set; }

    /// <summary>
    /// Price amount in cents.
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// Currency code.
    /// </summary>
    public string Currency { get; set; } = "usd";

    /// <summary>
    /// Billing interval.
    /// </summary>
    public string Interval { get; set; } = "month";
}
