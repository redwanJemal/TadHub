using TadHub.SharedKernel.Entities;

namespace Subscription.Core.Entities;

/// <summary>
/// Records usage for metered billing.
/// </summary>
public class TenantUsageRecord : TenantScopedEntity
{
    /// <summary>
    /// Associated subscription.
    /// </summary>
    public Guid TenantSubscriptionId { get; set; }
    public TenantSubscription TenantSubscription { get; set; } = null!;

    /// <summary>
    /// Usage metric key (e.g., "api_calls").
    /// </summary>
    public string MetricKey { get; set; } = string.Empty;

    /// <summary>
    /// Quantity of usage.
    /// </summary>
    public long Quantity { get; set; }

    /// <summary>
    /// Billing period this usage belongs to.
    /// </summary>
    public DateTimeOffset PeriodStart { get; set; }
    public DateTimeOffset PeriodEnd { get; set; }

    /// <summary>
    /// Whether this has been reported to Stripe.
    /// </summary>
    public bool ReportedToStripe { get; set; } = false;

    /// <summary>
    /// Stripe Usage Record ID (if reported).
    /// </summary>
    public string? StripeUsageRecordId { get; set; }
}
