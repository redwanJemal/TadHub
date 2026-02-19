using TadHub.SharedKernel.Entities;

namespace ClientManagement.Core.Entities;

/// <summary>
/// Discount card associated with a client.
/// UAE has special discount programs for certain groups.
/// </summary>
public class DiscountCard : TenantScopedEntity
{
    /// <summary>
    /// Client this card belongs to.
    /// </summary>
    public Guid ClientId { get; set; }

    /// <summary>
    /// Navigation property for client.
    /// </summary>
    public Client? Client { get; set; }

    /// <summary>
    /// Type of discount card.
    /// </summary>
    public DiscountCardType CardType { get; set; }

    /// <summary>
    /// Card number.
    /// </summary>
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>
    /// Discount percentage (0-100).
    /// </summary>
    public decimal DiscountPercentage { get; set; }

    /// <summary>
    /// Card validity end date.
    /// </summary>
    public DateTimeOffset? ValidUntil { get; set; }

    /// <summary>
    /// Whether the card is currently valid.
    /// </summary>
    public bool IsValid => ValidUntil == null || ValidUntil > DateTimeOffset.UtcNow;
}
