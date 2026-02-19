using TadHub.SharedKernel.Entities;

namespace Subscription.Core.Entities;

/// <summary>
/// Represents a credit ledger entry for a tenant.
/// Append-only - never update or delete entries.
/// </summary>
public class Credit : TenantScopedEntity
{
    /// <summary>
    /// Credit transaction type: purchase, bonus, spend, refund, expire.
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Amount of credits (positive for additions, negative for deductions).
    /// </summary>
    public long Amount { get; set; }

    /// <summary>
    /// Running balance after this transaction.
    /// </summary>
    public long Balance { get; set; }

    /// <summary>
    /// Description of the transaction.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Reference ID (e.g., Stripe Payment Intent ID, feature usage ID).
    /// </summary>
    public string? ReferenceId { get; set; }

    /// <summary>
    /// Reference type (e.g., "stripe_payment", "api_usage").
    /// </summary>
    public string? ReferenceType { get; set; }

    /// <summary>
    /// User who performed or triggered this transaction.
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// When these credits expire (null = never).
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    /// Metadata (JSON).
    /// </summary>
    public string? Metadata { get; set; }
}
