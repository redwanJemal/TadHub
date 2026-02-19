using SaasKit.SharedKernel.Entities;

namespace Subscription.Core.Entities;

/// <summary>
/// Represents usage-based pricing for a plan (e.g., $0.01 per API call over limit).
/// </summary>
public class PlanUsageBasedPrice : BaseEntity
{
    /// <summary>
    /// Associated plan.
    /// </summary>
    public Guid PlanId { get; set; }
    public Plan Plan { get; set; } = null!;

    /// <summary>
    /// Usage metric key (e.g., "api_calls", "storage_gb").
    /// </summary>
    public string MetricKey { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the metric.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Unit name (e.g., "call", "GB").
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Price per unit in cents.
    /// </summary>
    public long PricePerUnit { get; set; }

    /// <summary>
    /// Currency code.
    /// </summary>
    public string Currency { get; set; } = "usd";

    /// <summary>
    /// Included units (free tier).
    /// </summary>
    public long IncludedUnits { get; set; } = 0;

    /// <summary>
    /// Stripe metered Price ID.
    /// </summary>
    public string? StripePriceId { get; set; }
}
