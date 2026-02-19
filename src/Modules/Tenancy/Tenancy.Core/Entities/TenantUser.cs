using Identity.Core.Entities;
using TadHub.SharedKernel.Entities;
using Tenancy.Contracts.DTOs;

namespace Tenancy.Core.Entities;

/// <summary>
/// Tenant membership - links users to tenants with a role.
/// </summary>
public class TenantUser : BaseEntity
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
    /// Role within this tenant (owner, admin, member).
    /// </summary>
    public TenantRole Role { get; set; } = TenantRole.Member;

    /// <summary>
    /// When the user joined this tenant.
    /// </summary>
    public DateTimeOffset JoinedAt { get; set; }
}
