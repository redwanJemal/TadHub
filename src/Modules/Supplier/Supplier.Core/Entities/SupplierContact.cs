using TadHub.SharedKernel.Entities;

namespace Supplier.Core.Entities;

/// <summary>
/// Contact person for a supplier. Global entity (not tenant-scoped).
/// A contact can optionally be linked to a UserProfile if they have a system account.
/// </summary>
public class SupplierContact : BaseEntity
{
    /// <summary>
    /// The supplier this contact belongs to.
    /// </summary>
    public Guid SupplierId { get; set; }

    /// <summary>
    /// Navigation property to the supplier.
    /// </summary>
    public Supplier Supplier { get; set; } = null!;

    /// <summary>
    /// Optional link to a UserProfile (FK maintained at DB level, no CLR nav property).
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Full name of the contact person. Required.
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Contact email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Contact phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Job title or role at the supplier company.
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// Whether this is the primary contact for the supplier.
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Whether this contact is active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
