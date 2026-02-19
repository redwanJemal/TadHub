using TadHub.SharedKernel.Entities;

namespace Portal.Core.Entities;

/// <summary>
/// Represents an invitation to join a portal.
/// </summary>
public class PortalInvitation : TenantScopedEntity
{
    /// <summary>
    /// The portal this invitation is for.
    /// </summary>
    public Guid PortalId { get; set; }
    public Portal Portal { get; set; } = null!;

    /// <summary>
    /// Invited email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Invitation token.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Invitation status: pending, accepted, expired, revoked.
    /// </summary>
    public string Status { get; set; } = "pending";

    /// <summary>
    /// When the invitation expires.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    /// When the invitation was accepted.
    /// </summary>
    public DateTimeOffset? AcceptedAt { get; set; }

    /// <summary>
    /// User ID of who sent the invitation (tenant admin).
    /// </summary>
    public Guid InvitedByUserId { get; set; }

    /// <summary>
    /// Portal user created from this invitation.
    /// </summary>
    public Guid? PortalUserId { get; set; }
    public PortalUser? PortalUser { get; set; }

    /// <summary>
    /// Custom message included in invitation.
    /// </summary>
    public string? Message { get; set; }
}
