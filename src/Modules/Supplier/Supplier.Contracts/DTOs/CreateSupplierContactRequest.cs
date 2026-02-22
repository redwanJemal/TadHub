using System.ComponentModel.DataAnnotations;

namespace Supplier.Contracts.DTOs;

/// <summary>
/// Request to create a new supplier contact.
/// </summary>
public sealed record CreateSupplierContactRequest
{
    /// <summary>
    /// Full name of the contact person. Required.
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string FullName { get; init; } = string.Empty;

    /// <summary>
    /// Contact email address.
    /// </summary>
    [EmailAddress]
    [MaxLength(255)]
    public string? Email { get; init; }

    /// <summary>
    /// Contact phone number.
    /// </summary>
    [Phone]
    [MaxLength(50)]
    public string? Phone { get; init; }

    /// <summary>
    /// Job title or role at the supplier company.
    /// </summary>
    [MaxLength(100)]
    public string? JobTitle { get; init; }

    /// <summary>
    /// Whether this is the primary contact for the supplier.
    /// </summary>
    public bool IsPrimary { get; init; }

    /// <summary>
    /// Optional link to a UserProfile if the contact has a system account.
    /// </summary>
    public Guid? UserId { get; init; }
}
