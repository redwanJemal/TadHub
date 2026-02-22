using System.ComponentModel.DataAnnotations;

namespace Supplier.Contracts.DTOs;

/// <summary>
/// Request to link a supplier to a tenant.
/// </summary>
public sealed record LinkSupplierRequest
{
    /// <summary>
    /// The global supplier ID to link to this tenant.
    /// </summary>
    [Required]
    public Guid SupplierId { get; init; }

    /// <summary>
    /// Reference number for the contract or agreement.
    /// </summary>
    [MaxLength(100)]
    public string? ContractReference { get; init; }

    /// <summary>
    /// Internal notes about this tenant-supplier relationship.
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; init; }

    /// <summary>
    /// Start date of the supplier agreement.
    /// </summary>
    public DateTimeOffset? AgreementStartDate { get; init; }

    /// <summary>
    /// End date of the supplier agreement.
    /// </summary>
    public DateTimeOffset? AgreementEndDate { get; init; }
}
