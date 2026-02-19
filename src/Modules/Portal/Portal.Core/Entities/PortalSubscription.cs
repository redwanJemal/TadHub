using SaasKit.SharedKernel.Entities;

namespace Portal.Core.Entities;

/// <summary>
/// Represents a portal user's subscription (if portal offers paid features).
/// </summary>
public class PortalSubscription : TenantScopedEntity
{
    /// <summary>
    /// The portal this subscription belongs to.
    /// </summary>
    public Guid PortalId { get; set; }
    public Portal Portal { get; set; } = null!;

    /// <summary>
    /// The portal user with this subscription.
    /// </summary>
    public Guid PortalUserId { get; set; }
    public PortalUser PortalUser { get; set; } = null!;

    /// <summary>
    /// Plan name/tier.
    /// </summary>
    public string PlanName { get; set; } = string.Empty;

    /// <summary>
    /// Subscription status: active, trialing, past_due, canceled.
    /// </summary>
    public string Status { get; set; } = "active";

    /// <summary>
    /// Stripe Subscription ID (via Stripe Connect).
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
    /// Trial end date.
    /// </summary>
    public DateTimeOffset? TrialEnd { get; set; }

    /// <summary>
    /// When canceled.
    /// </summary>
    public DateTimeOffset? CanceledAt { get; set; }

    /// <summary>
    /// Whether to cancel at period end.
    /// </summary>
    public bool CancelAtPeriodEnd { get; set; } = false;
}
