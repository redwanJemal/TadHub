using SaasKit.SharedKernel.Entities;
using Tenancy.Contracts.DTOs;

namespace Tenancy.Core.Entities;

/// <summary>
/// Tenant entity representing an organization/workspace.
/// </summary>
public class Tenant : BaseEntity
{
    /// <summary>
    /// Display name of the tenant.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL-friendly unique identifier (e.g., "acme-corp").
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the tenant.
    /// </summary>
    public TenantStatus Status { get; set; } = TenantStatus.Active;

    /// <summary>
    /// Optional tenant type for categorization.
    /// </summary>
    public Guid? TenantTypeId { get; set; }

    /// <summary>
    /// Navigation property for tenant type.
    /// </summary>
    public TenantType? TenantType { get; set; }

    /// <summary>
    /// Logo URL for branding.
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Description of the tenant/organization.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Website URL.
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// JSON settings for tenant-specific configuration.
    /// </summary>
    public string? Settings { get; set; }

    /// <summary>
    /// Navigation property for tenant members.
    /// </summary>
    public ICollection<TenantUser> Members { get; set; } = new List<TenantUser>();

    /// <summary>
    /// Navigation property for invitations.
    /// </summary>
    public ICollection<TenantUserInvitation> Invitations { get; set; } = new List<TenantUserInvitation>();

    /// <summary>
    /// Parent tenant ID for hierarchical tenants.
    /// </summary>
    public Guid? ParentTenantId { get; set; }

    /// <summary>
    /// Navigation property for parent tenant.
    /// </summary>
    public Tenant? ParentTenant { get; set; }

    /// <summary>
    /// Navigation property for child tenants.
    /// </summary>
    public ICollection<Tenant> ChildTenants { get; set; } = new List<Tenant>();
}
