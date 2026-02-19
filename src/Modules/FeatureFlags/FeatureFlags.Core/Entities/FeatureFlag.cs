using SaasKit.SharedKernel.Entities;

namespace FeatureFlags.Core.Entities;

/// <summary>
/// Global feature flag definition.
/// </summary>
public class FeatureFlag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEnabled { get; set; } = false;
    public int? Percentage { get; set; }
    public string? AllowedPlans { get; set; } // JSON array
    public string? AllowedTenantIds { get; set; } // JSON array
    public DateTimeOffset? EnabledAt { get; set; }
    public DateTimeOffset? DisabledAt { get; set; }
    public ICollection<FeatureFlagFilter> Filters { get; set; } = new List<FeatureFlagFilter>();
}

public class FeatureFlagFilter : BaseEntity
{
    public Guid FeatureFlagId { get; set; }
    public FeatureFlag FeatureFlag { get; set; } = null!;
    public string Type { get; set; } = string.Empty; // Plan, TenantType, Percentage
    public string Value { get; set; } = string.Empty;
}
