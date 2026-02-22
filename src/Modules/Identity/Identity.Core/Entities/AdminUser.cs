using TadHub.SharedKernel.Entities;

namespace Identity.Core.Entities;

/// <summary>
/// Platform staff member. Users with this record have platform-level administrative access.
/// </summary>
public class PlatformStaff : BaseEntity
{
    public Guid UserId { get; set; }
    public UserProfile User { get; set; } = null!;

    /// <summary>
    /// Platform role mirrored from Keycloak realm roles.
    /// Values: "super-admin", "admin", "finance", "sales", "support"
    /// </summary>
    public string Role { get; set; } = "admin";

    /// <summary>
    /// Optional department/notes for organizational clarity.
    /// </summary>
    public string? Department { get; set; }
}
