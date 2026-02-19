using SaasKit.SharedKernel.Entities;

namespace Subscription.Core.Entities;

/// <summary>
/// Represents a price for a plan (e.g., monthly, yearly).
/// </summary>
public class PlanPrice : BaseEntity
{
    /// <summary>
    /// Associated plan.
    /// </summary>
    public Guid PlanId { get; set; }
    public Plan Plan { get; set; } = null!;

    /// <summary>
    /// Price amount in cents (e.g., 1999 = $19.99).
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// Currency code (e.g., "usd", "eur").
    /// </summary>
    public string Currency { get; set; } = "usd";

    /// <summary>
    /// Billing interval: month, year.
    /// </summary>
    public string Interval { get; set; } = "month";

    /// <summary>
    /// Interval count (e.g., 1 for monthly, 12 for yearly).
    /// </summary>
    public int IntervalCount { get; set; } = 1;

    /// <summary>
    /// Trial period in days (0 = no trial).
    /// </summary>
    public int TrialDays { get; set; } = 0;

    /// <summary>
    /// Whether this price is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Stripe Price ID.
    /// </summary>
    public string? StripePriceId { get; set; }
}
