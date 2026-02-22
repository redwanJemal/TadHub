using TadHub.SharedKernel.Entities;
using TadHub.SharedKernel.Enums;
using Tenancy.Contracts.DTOs;

namespace Tenancy.Core.Entities;

/// <summary>
/// Tenant entity representing a Tadbeer center/organization.
/// </summary>
public class Tenant : BaseEntity
{
    /// <summary>
    /// Display name of the tenant.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Arabic name for bilingual support.
    /// </summary>
    public string? NameAr { get; set; }

    /// <summary>
    /// URL-friendly unique identifier (e.g., "acme-corp").
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the tenant.
    /// </summary>
    public TenantStatus Status { get; set; } = TenantStatus.Active;

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
    public ICollection<TenantMembership> Members { get; set; } = new List<TenantMembership>();

    /// <summary>
    /// Navigation property for invitations.
    /// </summary>
    public ICollection<TenantUserInvitation> Invitations { get; set; } = new List<TenantUserInvitation>();

    #region Tadbeer Agency Fields

    /// <summary>
    /// Tadbeer license number (unique identifier for the agency).
    /// </summary>
    public string? TadbeerLicenseNumber { get; set; }

    /// <summary>
    /// MoHRE (Ministry of Human Resources and Emiratisation) license number.
    /// </summary>
    public string? MohreLicenseNumber { get; set; }

    /// <summary>
    /// Trade license number.
    /// </summary>
    public string? TradeLicenseNumber { get; set; }

    /// <summary>
    /// Trade license expiry date.
    /// </summary>
    public DateTimeOffset? TradeLicenseExpiry { get; set; }

    /// <summary>
    /// UAE Emirate where the agency is registered.
    /// </summary>
    public Emirate? Emirate { get; set; }

    /// <summary>
    /// Whether the agency is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Primary license expiry date.
    /// </summary>
    public DateTimeOffset? LicenseExpiryDate { get; set; }

    /// <summary>
    /// Tax Registration Number (TRN) for VAT compliance.
    /// </summary>
    public string? TaxRegistrationNumber { get; set; }

    /// <summary>
    /// Navigation property for agency licenses.
    /// </summary>
    public ICollection<TadbeerLicense> Licenses { get; set; } = new List<TadbeerLicense>();

    /// <summary>
    /// Navigation property for shared pool agreements where this tenant is the provider.
    /// </summary>
    public ICollection<SharedPoolAgreement> OutgoingPoolAgreements { get; set; } = new List<SharedPoolAgreement>();

    /// <summary>
    /// Navigation property for shared pool agreements where this tenant is the receiver.
    /// </summary>
    public ICollection<SharedPoolAgreement> IncomingPoolAgreements { get; set; } = new List<SharedPoolAgreement>();

    #endregion
}
