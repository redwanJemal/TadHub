using System.ComponentModel.DataAnnotations;

namespace Supplier.Contracts.DTOs;

/// <summary>
/// Request to update an existing supplier.
/// All fields are optional - only non-null values are updated.
/// </summary>
public sealed record UpdateSupplierRequest
{
    /// <summary>
    /// Supplier name in English.
    /// </summary>
    [MaxLength(255)]
    public string? NameEn { get; init; }

    /// <summary>
    /// Supplier name in Arabic.
    /// </summary>
    [MaxLength(255)]
    public string? NameAr { get; init; }

    /// <summary>
    /// Country ISO alpha-2 code.
    /// </summary>
    [MaxLength(10)]
    public string? Country { get; init; }

    /// <summary>
    /// City where the supplier is located.
    /// </summary>
    [MaxLength(100)]
    public string? City { get; init; }

    /// <summary>
    /// Business license or trade registration number.
    /// </summary>
    [MaxLength(100)]
    public string? LicenseNumber { get; init; }

    /// <summary>
    /// Primary phone number.
    /// </summary>
    [Phone]
    [MaxLength(50)]
    public string? Phone { get; init; }

    /// <summary>
    /// Primary email address.
    /// </summary>
    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; init; }

    /// <summary>
    /// Supplier website URL.
    /// </summary>
    [Url]
    [MaxLength(500)]
    public string? Website { get; init; }

    /// <summary>
    /// Internal notes about the supplier.
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; init; }

    /// <summary>
    /// Whether the supplier is active. Null means no change.
    /// </summary>
    public bool? IsActive { get; init; }
}
