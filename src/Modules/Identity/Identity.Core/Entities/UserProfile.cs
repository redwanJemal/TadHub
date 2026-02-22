using TadHub.SharedKernel.Entities;

namespace Identity.Core.Entities;

/// <summary>
/// User profile entity. Synced from Keycloak and enriched with application-specific data.
/// This is a global entity (not tenant-scoped) as users can belong to multiple tenants.
/// </summary>
public class UserProfile : BaseEntity
{
    /// <summary>
    /// Keycloak user ID (sub claim). Unique identifier in Keycloak.
    /// </summary>
    public string KeycloakId { get; set; } = string.Empty;

    /// <summary>
    /// User's email address. Must be unique.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Computed full name.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// URL to the user's avatar image.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// User's phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// User's preferred locale (e.g., "en", "de", "ar").
    /// </summary>
    public string Locale { get; set; } = "en";

    /// <summary>
    /// Default tenant for this user. Set when user logs in without tenant context.
    /// </summary>
    public Guid? DefaultTenantId { get; set; }

    /// <summary>
    /// Whether the user account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Last successful login timestamp.
    /// </summary>
    public DateTimeOffset? LastLoginAt { get; set; }

    /// <summary>
    /// Navigation property for platform staff record (if this user is platform staff).
    /// </summary>
    public PlatformStaff? PlatformStaff { get; set; }
}
