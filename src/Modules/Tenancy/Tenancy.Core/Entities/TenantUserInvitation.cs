using TadHub.SharedKernel.Entities;

namespace Tenancy.Core.Entities;

/// <summary>
/// Invitation to join a tenant.
/// </summary>
public class TenantUserInvitation : BaseEntity
{
    /// <summary>
    /// The tenant this invitation is for.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Navigation property for tenant.
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Email address the invitation was sent to.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Optional RBAC role ID to assign when invitation is accepted.
    /// FK to Authorization.Role. Null defaults to "Member" role.
    /// </summary>
    public Guid? DefaultRoleId { get; set; }

    /// <summary>
    /// Unique token for accepting the invitation.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// When the invitation expires.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    /// When the invitation was accepted (null if pending).
    /// </summary>
    public DateTimeOffset? AcceptedAt { get; set; }

    /// <summary>
    /// User who sent the invitation (FK to user_profiles — no navigation to avoid cross-module dependency).
    /// </summary>
    public Guid InvitedByUserId { get; set; }

    /// <summary>
    /// Whether the invitation has expired.
    /// </summary>
    public bool IsExpired => DateTimeOffset.UtcNow > ExpiresAt;

    /// <summary>
    /// Whether the invitation has been accepted.
    /// </summary>
    public bool IsAccepted => AcceptedAt.HasValue;
}
