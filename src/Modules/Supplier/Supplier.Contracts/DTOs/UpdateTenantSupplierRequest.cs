using System.ComponentModel.DataAnnotations;

namespace Supplier.Contracts.DTOs;

/// <summary>
/// Request to update a tenant-supplier relationship.
/// All fields are optional - only non-null values are updated.
/// </summary>
public sealed record UpdateTenantSupplierRequest
{
    /// <summary>
    /// Updated status. Must be one of: Active, Suspended, Terminated.
    /// </summary>
    [MaxLength(20)]
    public string? Status { get; init; }

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
