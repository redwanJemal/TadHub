using TadHub.SharedKernel.Entities;

namespace Subscription.Core.Entities;

/// <summary>
/// Represents a product/add-on in a tenant's subscription.
/// </summary>
public class TenantSubscriptionProduct : TenantScopedEntity
{
    /// <summary>
    /// Parent subscription.
    /// </summary>
    public Guid TenantSubscriptionId { get; set; }
    public TenantSubscription TenantSubscription { get; set; } = null!;

    /// <summary>
    /// Product name/identifier.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Stripe Subscription Item ID.
    /// </summary>
    public string? StripeSubscriptionItemId { get; set; }

    /// <summary>
    /// Quantity (for seat-based products).
    /// </summary>
    public int Quantity { get; set; } = 1;

    /// <summary>
    /// Product prices.
    /// </summary>
    public ICollection<TenantSubscriptionPrice> Prices { get; set; } = new List<TenantSubscriptionPrice>();
}
