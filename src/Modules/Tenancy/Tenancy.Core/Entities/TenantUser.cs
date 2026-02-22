using Identity.Core.Entities;
using TadHub.SharedKernel.Entities;
using Tenancy.Contracts.DTOs;

namespace Tenancy.Core.Entities;

/// <summary>
/// Tenant membership - links users to tenants. Structural relationship only.
/// Authorization is handled by the Authorization module (Role → Permissions).
/// </summary>
public class TenantMembership : BaseEntity
{
    /// <summary>
    /// The tenant ID.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Navigation property for tenant.
    /// </summary>
    public Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// The user ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property for user.
    /// </summary>
    public UserProfile User { get; set; } = null!;

    /// <summary>
    /// Structural ownership — governance, not permissions.
    /// </summary>
    public bool IsOwner { get; set; }

    /// <summary>
    /// Membership status.
    /// </summary>
    public MembershipStatus Status { get; set; } = MembershipStatus.Active;

    /// <summary>
    /// When the user joined this tenant.
    /// </summary>
    public DateTimeOffset JoinedAt { get; set; }
}
