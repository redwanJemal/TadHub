using TadHub.SharedKernel.Entities;

namespace Authorization.Core.Entities;

/// <summary>
/// Many-to-many relationship between Role and Permission.
/// </summary>
public class RolePermission : BaseEntity
{
    /// <summary>
    /// The role ID.
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// Navigation property for the role.
    /// </summary>
    public Role Role { get; set; } = null!;

    /// <summary>
    /// The permission ID.
    /// </summary>
    public Guid PermissionId { get; set; }

    /// <summary>
    /// Navigation property for the permission.
    /// </summary>
    public Permission Permission { get; set; } = null!;
}
