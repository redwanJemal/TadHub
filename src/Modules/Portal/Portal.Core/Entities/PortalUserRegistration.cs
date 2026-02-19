using TadHub.SharedKernel.Entities;

namespace Portal.Core.Entities;

/// <summary>
/// Tracks portal user registration requests (for email verification flow).
/// </summary>
public class PortalUserRegistration : TenantScopedEntity
{
    /// <summary>
    /// The portal this registration is for.
    /// </summary>
    public Guid PortalId { get; set; }
    public Portal Portal { get; set; } = null!;

    /// <summary>
    /// Email address being registered.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Hashed password.
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// First name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Email verification token.
    /// </summary>
    public string VerificationToken { get; set; } = string.Empty;

    /// <summary>
    /// Registration status: pending, verified, expired.
    /// </summary>
    public string Status { get; set; } = "pending";

    /// <summary>
    /// When the registration expires.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    /// When the email was verified.
    /// </summary>
    public DateTimeOffset? VerifiedAt { get; set; }

    /// <summary>
    /// Portal user created from this registration.
    /// </summary>
    public Guid? PortalUserId { get; set; }
    public PortalUser? PortalUser { get; set; }

    /// <summary>
    /// IP address of registration request.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent of registration request.
    /// </summary>
    public string? UserAgent { get; set; }
}
