using TadHub.SharedKernel.Entities;

namespace Identity.Core.Entities;

/// <summary>
/// Platform admin user. Users with this record have platform-level administrative access.
/// </summary>
public class AdminUser : BaseEntity
{
    /// <summary>
    /// Reference to the user profile.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to the user profile.
    /// </summary>
    public UserProfile User { get; set; } = null!;

    /// <summary>
    /// Whether this admin has super admin privileges.
    /// Super admins can manage other admins and have unrestricted access.
    /// </summary>
    public bool IsSuperAdmin { get; set; }
}
