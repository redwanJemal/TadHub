using TadHub.SharedKernel.Entities;

namespace Authorization.Core.Entities;

/// <summary>
/// Template for tenant roles. When a tenant is created, roles are cloned from templates.
/// System templates cannot be deleted.
/// </summary>
public class RoleTemplate : BaseEntity
{
    /// <summary>
    /// Template name (e.g., "Owner", "Admin", "Accountant").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description of the template.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this is a system template that cannot be deleted or renamed.
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Display order for UI.
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Navigation property for template-permission mappings.
    /// </summary>
    public ICollection<RoleTemplatePermission> Permissions { get; set; } = new List<RoleTemplatePermission>();
}

/// <summary>
/// Many-to-many relationship between RoleTemplate and Permission.
/// </summary>
public class RoleTemplatePermission : BaseEntity
{
    /// <summary>
    /// The role template ID.
    /// </summary>
    public Guid TemplateId { get; set; }

    /// <summary>
    /// Navigation property for the role template.
    /// </summary>
    public RoleTemplate Template { get; set; } = null!;

    /// <summary>
    /// The permission ID.
    /// </summary>
    public Guid PermissionId { get; set; }

    /// <summary>
    /// Navigation property for the permission.
    /// </summary>
    public Permission Permission { get; set; } = null!;
}
