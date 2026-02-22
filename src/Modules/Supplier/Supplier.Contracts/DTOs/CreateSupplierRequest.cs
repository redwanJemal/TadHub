using System.ComponentModel.DataAnnotations;

namespace Supplier.Contracts.DTOs;

/// <summary>
/// Request to create a new supplier.
/// </summary>
public sealed record CreateSupplierRequest
{
    /// <summary>
    /// Supplier name in English. Required.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string NameEn { get; init; } = string.Empty;

    /// <summary>
    /// Supplier name in Arabic. Optional.
    /// </summary>
    [MaxLength(255)]
    public string? NameAr { get; init; }

    /// <summary>
    /// Country ISO alpha-2 code. Required.
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string Country { get; init; } = string.Empty;

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
}
