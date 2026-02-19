using TadHub.SharedKernel.Entities;

namespace Authorization.Core.Entities;

/// <summary>
/// User group within a tenant for bulk role assignment.
/// </summary>
public class Group : TenantScopedEntity
{
    /// <summary>
    /// Group name (unique within tenant).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Navigation property for group members.
    /// </summary>
    public ICollection<GroupUser> Members { get; set; } = new List<GroupUser>();

    /// <summary>
    /// Navigation property for roles assigned to this group.
    /// </summary>
    public ICollection<GroupRole> Roles { get; set; } = new List<GroupRole>();
}

/// <summary>
/// Assignment of a role to a group.
/// </summary>
public class GroupRole : BaseEntity
{
    /// <summary>
    /// The group ID.
    /// </summary>
    public Guid GroupId { get; set; }

    /// <summary>
    /// Navigation property for the group.
    /// </summary>
    public Group Group { get; set; } = null!;

    /// <summary>
    /// The role ID.
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// Navigation property for the role.
    /// </summary>
    public Role Role { get; set; } = null!;
}
