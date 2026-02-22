using TadHub.SharedKernel.Entities;

namespace Supplier.Core.Entities;

/// <summary>
/// Supplier entity. Represents a global supplier record that can be linked to multiple tenants.
/// This is a global entity (not tenant-scoped) as suppliers can serve multiple tenants.
/// </summary>
public class Supplier : BaseEntity
{
    /// <summary>
    /// Supplier name in English. Required.
    /// </summary>
    public string NameEn { get; set; } = string.Empty;

    /// <summary>
    /// Supplier name in Arabic. Optional.
    /// </summary>
    public string? NameAr { get; set; }

    /// <summary>
    /// Country ISO alpha-2 code (e.g., "AE", "PH").
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// City where the supplier is located.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Business license or trade registration number.
    /// </summary>
    public string? LicenseNumber { get; set; }

    /// <summary>
    /// Primary phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Primary email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Supplier website URL.
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// Internal notes about the supplier.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Whether the supplier is active in the system.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Contacts associated with this supplier.
    /// </summary>
    public ICollection<SupplierContact> Contacts { get; set; } = new List<SupplierContact>();

    /// <summary>
    /// Tenant relationships for this supplier.
    /// </summary>
    public ICollection<TenantSupplier> TenantRelationships { get; set; } = new List<TenantSupplier>();
}
