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
    /// The user ID (FK to user_profiles — no navigation to avoid cross-module dependency).
    /// </summary>
    public Guid UserId { get; set; }

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
