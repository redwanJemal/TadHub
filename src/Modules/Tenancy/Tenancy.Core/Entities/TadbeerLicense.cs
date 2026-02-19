using TadHub.SharedKernel.Entities;

namespace Tenancy.Core.Entities;

/// <summary>
/// Represents a license held by a Tadbeer agency.
/// One tenant can have multiple licenses (Tadbeer, MoHRE, Trade).
/// </summary>
public class TadbeerLicense : TenantScopedEntity
{
    /// <summary>
    /// Type of license.
    /// </summary>
    public LicenseType LicenseType { get; set; }

    /// <summary>
    /// License number.
    /// </summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Date the license was issued.
    /// </summary>
    public DateTimeOffset IssuedAt { get; set; }

    /// <summary>
    /// Date the license expires.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    /// Current status of the license.
    /// </summary>
    public LicenseStatus Status { get; set; } = LicenseStatus.Active;

    /// <summary>
    /// URL to the license document (stored in MinIO/S3).
    /// </summary>
    public string? DocumentUrl { get; set; }

    /// <summary>
    /// Additional notes about the license.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Navigation property for the tenant.
    /// </summary>
    public Tenant? Tenant { get; set; }

    /// <summary>
    /// Check if the license is currently valid.
    /// </summary>
    public bool IsValid => Status == LicenseStatus.Active && ExpiresAt > DateTimeOffset.UtcNow;

    /// <summary>
    /// Days until expiry (negative if expired).
    /// </summary>
    public int DaysUntilExpiry => (int)(ExpiresAt - DateTimeOffset.UtcNow).TotalDays;
}
