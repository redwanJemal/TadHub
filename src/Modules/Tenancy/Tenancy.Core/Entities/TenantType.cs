using SaasKit.SharedKernel.Entities;

namespace Tenancy.Core.Entities;

/// <summary>
/// Tenant type for categorization (e.g., Agency, Brand, Enterprise).
/// </summary>
public class TenantType : BaseEntity
{
    /// <summary>
    /// Type name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Display order for UI.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Whether this type is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Navigation property for tenants of this type.
    /// </summary>
    public ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();

    /// <summary>
    /// Allowed child tenant types (for hierarchies).
    /// </summary>
    public ICollection<TenantTypeRelationship> AllowedChildTypes { get; set; } = new List<TenantTypeRelationship>();

    /// <summary>
    /// Parent types that can contain this type.
    /// </summary>
    public ICollection<TenantTypeRelationship> ParentTypes { get; set; } = new List<TenantTypeRelationship>();
}

/// <summary>
/// Defines allowed parent-child relationships between tenant types.
/// </summary>
public class TenantTypeRelationship : BaseEntity
{
    /// <summary>
    /// Parent tenant type ID.
    /// </summary>
    public Guid ParentTypeId { get; set; }

    /// <summary>
    /// Navigation property for parent type.
    /// </summary>
    public TenantType ParentType { get; set; } = null!;

    /// <summary>
    /// Child tenant type ID.
    /// </summary>
    public Guid ChildTypeId { get; set; }

    /// <summary>
    /// Navigation property for child type.
    /// </summary>
    public TenantType ChildType { get; set; } = null!;
}
