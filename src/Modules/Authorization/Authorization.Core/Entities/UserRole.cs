using Identity.Core.Entities;
using SaasKit.SharedKernel.Entities;

namespace Authorization.Core.Entities;

/// <summary>
/// Assignment of a user to a role within a tenant.
/// </summary>
public class UserRole : TenantScopedEntity
{
    /// <summary>
    /// The user ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property for the user.
    /// </summary>
    public UserProfile User { get; set; } = null!;

    /// <summary>
    /// The role ID.
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// Navigation property for the role.
    /// </summary>
    public Role Role { get; set; } = null!;

    /// <summary>
    /// When the role was assigned.
    /// </summary>
    public DateTimeOffset AssignedAt { get; set; }

    /// <summary>
    /// Who assigned this role (null for system assignments).
    /// </summary>
    public Guid? AssignedByUserId { get; set; }
}
