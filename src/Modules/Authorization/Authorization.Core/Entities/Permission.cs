using TadHub.SharedKernel.Entities;

namespace Authorization.Core.Entities;

/// <summary>
/// Global permission definition.
/// Permissions are predefined and seeded on startup.
/// </summary>
public class Permission : BaseEntity
{
    /// <summary>
    /// Unique permission name (e.g., "tenancy.manage", "users.view").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Module this permission belongs to (e.g., "tenancy", "users", "billing").
    /// </summary>
    public string Module { get; set; } = string.Empty;

    /// <summary>
    /// Display order within the module.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Navigation property for role-permission mappings.
    /// </summary>
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
