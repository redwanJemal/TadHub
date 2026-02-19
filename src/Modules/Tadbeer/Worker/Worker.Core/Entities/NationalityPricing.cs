using TadHub.SharedKernel.Entities;

namespace Worker.Core.Entities;

/// <summary>
/// Nationality-based pricing.
/// Supports time-ranged pricing per MoHRE 6-month revision cycle.
/// </summary>
public class NationalityPricing : TenantScopedEntity
{
    /// <summary>
    /// Nationality (e.g., "Philippines", "Indonesia", "Ethiopia").
    /// </summary>
    public string Nationality { get; set; } = string.Empty;

    /// <summary>
    /// Contract type this pricing applies to.
    /// </summary>
    public ContractType ContractType { get; set; }

    /// <summary>
    /// Price amount.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Currency (default AED).
    /// </summary>
    public string Currency { get; set; } = "AED";

    /// <summary>
    /// When this pricing becomes effective.
    /// </summary>
    public DateTimeOffset EffectiveFrom { get; set; }

    /// <summary>
    /// When this pricing expires (null = no expiry).
    /// </summary>
    public DateTimeOffset? EffectiveTo { get; set; }

    /// <summary>
    /// Whether this pricing is currently active.
    /// </summary>
    public bool IsActiveAt(DateTimeOffset asOf) =>
        EffectiveFrom <= asOf && (EffectiveTo == null || EffectiveTo > asOf);
}
