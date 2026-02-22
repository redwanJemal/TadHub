using TadHub.SharedKernel.Entities;

namespace Authorization.Core.Entities;

/// <summary>
/// Tenant-scoped role. Each tenant has its own set of roles.
/// Default roles (Owner, Admin, Member) are created when a tenant is created.
/// </summary>
public class Role : TenantScopedEntity
{
    /// <summary>
    /// Role name (unique within tenant).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this is a default role (created automatically for new tenants).
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Whether this is a system role that cannot be deleted or renamed.
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Display order for UI.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// The role template this role was created from (null if custom).
    /// </summary>
    public Guid? TemplateId { get; set; }

    /// <summary>
    /// Navigation property for the source role template.
    /// </summary>
    public RoleTemplate? Template { get; set; }

    /// <summary>
    /// Whether this is a custom role (not created from a template).
    /// </summary>
    public bool IsCustom { get; set; }

    /// <summary>
    /// Navigation property for role-permission mappings.
    /// </summary>
    public ICollection<RolePermission> Permissions { get; set; } = new List<RolePermission>();

    /// <summary>
    /// Navigation property for user-role assignments.
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
