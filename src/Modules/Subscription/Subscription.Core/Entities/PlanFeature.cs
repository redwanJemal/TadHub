using TadHub.SharedKernel.Entities;

namespace Subscription.Core.Entities;

/// <summary>
/// Represents a feature or limit included in a plan.
/// </summary>
public class PlanFeature : BaseEntity
{
    /// <summary>
    /// Associated plan.
    /// </summary>
    public Guid PlanId { get; set; }
    public Plan Plan { get; set; } = null!;

    /// <summary>
    /// Feature key (e.g., "max_users", "storage_gb", "api_calls_per_month").
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the feature.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Feature description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Value type: boolean, number, unlimited.
    /// </summary>
    public string ValueType { get; set; } = "boolean";

    /// <summary>
    /// Boolean value (for boolean features).
    /// </summary>
    public bool? BooleanValue { get; set; }

    /// <summary>
    /// Numeric value (for numeric limits).
    /// Null means unlimited.
    /// </summary>
    public long? NumericValue { get; set; }

    /// <summary>
    /// Whether this feature is unlimited.
    /// </summary>
    public bool IsUnlimited { get; set; } = false;

    /// <summary>
    /// Display order.
    /// </summary>
    public int DisplayOrder { get; set; }
}
