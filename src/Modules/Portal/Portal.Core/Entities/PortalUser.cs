using TadHub.SharedKernel.Entities;

namespace Portal.Core.Entities;

/// <summary>
/// Represents a user within a specific portal.
/// Portal users are separate from tenant users.
/// </summary>
public class PortalUser : TenantScopedEntity
{
    /// <summary>
    /// The portal this user belongs to.
    /// </summary>
    public Guid PortalId { get; set; }
    public Portal Portal { get; set; } = null!;

    /// <summary>
    /// User's email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Normalized email for lookups.
    /// </summary>
    public string NormalizedEmail { get; set; } = string.Empty;

    /// <summary>
    /// Whether the email is verified.
    /// </summary>
    public bool EmailVerified { get; set; } = false;

    /// <summary>
    /// Hashed password (null if using SSO only).
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// User's first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// User's last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Display name.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Avatar URL.
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// User's phone number.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Whether the user is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Last login timestamp.
    /// </summary>
    public DateTimeOffset? LastLoginAt { get; set; }

    /// <summary>
    /// Login count.
    /// </summary>
    public int LoginCount { get; set; } = 0;

    /// <summary>
    /// SSO subject identifier (if using SSO).
    /// </summary>
    public string? SsoSubject { get; set; }

    /// <summary>
    /// SSO provider name.
    /// </summary>
    public string? SsoProvider { get; set; }

    /// <summary>
    /// User metadata (JSON).
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// User's portal subscription (if applicable).
    /// </summary>
    public PortalSubscription? Subscription { get; set; }

    /// <summary>
    /// Full name helper.
    /// </summary>
    public string FullName => string.IsNullOrWhiteSpace(DisplayName)
        ? $"{FirstName} {LastName}".Trim()
        : DisplayName;
}
