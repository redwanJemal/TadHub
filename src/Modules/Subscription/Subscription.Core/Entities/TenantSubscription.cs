using TadHub.SharedKernel.Entities;

namespace Subscription.Core.Entities;

/// <summary>
/// Represents a tenant's active subscription.
/// </summary>
public class TenantSubscription : TenantScopedEntity
{
    /// <summary>
    /// Current plan.
    /// </summary>
    public Guid PlanId { get; set; }
    public Plan Plan { get; set; } = null!;

    /// <summary>
    /// Current price (billing interval).
    /// </summary>
    public Guid PlanPriceId { get; set; }
    public PlanPrice PlanPrice { get; set; } = null!;

    /// <summary>
    /// Subscription status: active, trialing, past_due, canceled, unpaid.
    /// </summary>
    public string Status { get; set; } = "active";

    /// <summary>
    /// Stripe Subscription ID.
    /// </summary>
    public string? StripeSubscriptionId { get; set; }

    /// <summary>
    /// Stripe Customer ID.
    /// </summary>
    public string? StripeCustomerId { get; set; }

    /// <summary>
    /// Current billing period start.
    /// </summary>
    public DateTimeOffset CurrentPeriodStart { get; set; }

    /// <summary>
    /// Current billing period end.
    /// </summary>
    public DateTimeOffset CurrentPeriodEnd { get; set; }

    /// <summary>
    /// Trial end date (if applicable).
    /// </summary>
    public DateTimeOffset? TrialEnd { get; set; }

    /// <summary>
    /// When the subscription was canceled (if applicable).
    /// </summary>
    public DateTimeOffset? CanceledAt { get; set; }

    /// <summary>
    /// Whether to cancel at period end.
    /// </summary>
    public bool CancelAtPeriodEnd { get; set; } = false;

    /// <summary>
    /// Subscription products (for multi-product subscriptions).
    /// </summary>
    public ICollection<TenantSubscriptionProduct> Products { get; set; } = new List<TenantSubscriptionProduct>();
}
