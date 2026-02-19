using SaasKit.SharedKernel.Entities;

namespace Subscription.Core.Entities;

/// <summary>
/// Represents a subscription plan (e.g., Free, Pro, Enterprise).
/// Global entity - not tenant-scoped.
/// </summary>
public class Plan : BaseEntity
{
    /// <summary>
    /// Plan name (e.g., "Pro", "Enterprise").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL-friendly slug (e.g., "pro", "enterprise").
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Plan description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this plan is currently active and available.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this is the default plan for new tenants.
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Display order for listing.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Stripe Product ID.
    /// </summary>
    public string? StripeProductId { get; set; }

    /// <summary>
    /// Plan prices (monthly, yearly, etc.).
    /// </summary>
    public ICollection<PlanPrice> Prices { get; set; } = new List<PlanPrice>();

    /// <summary>
    /// Plan features/limits.
    /// </summary>
    public ICollection<PlanFeature> Features { get; set; } = new List<PlanFeature>();

    /// <summary>
    /// Usage-based pricing components.
    /// </summary>
    public ICollection<PlanUsageBasedPrice> UsageBasedPrices { get; set; } = new List<PlanUsageBasedPrice>();
}
