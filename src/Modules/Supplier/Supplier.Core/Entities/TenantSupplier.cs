using TadHub.SharedKernel.Entities;

namespace Supplier.Core.Entities;

/// <summary>
/// Status of a tenant-supplier relationship.
/// </summary>
public enum SupplierRelationshipStatus
{
    /// <summary>
    /// The supplier relationship is active.
    /// </summary>
    Active = 0,

    /// <summary>
    /// The supplier relationship is temporarily suspended.
    /// </summary>
    Suspended = 1,

    /// <summary>
    /// The supplier relationship has been terminated.
    /// </summary>
    Terminated = 2
}

/// <summary>
/// Represents the relationship between a tenant and a supplier.
/// Tenant-scoped entity: each tenant maintains their own supplier relationships.
/// </summary>
public class TenantSupplier : TenantScopedEntity
{
    /// <summary>
    /// The global supplier this relationship refers to.
    /// </summary>
    public Guid SupplierId { get; set; }

    /// <summary>
    /// Navigation property to the supplier.
    /// </summary>
    public Supplier Supplier { get; set; } = null!;

    /// <summary>
    /// Current status of the supplier relationship.
    /// </summary>
    public SupplierRelationshipStatus Status { get; set; } = SupplierRelationshipStatus.Active;

    /// <summary>
    /// Reference number for the contract or agreement.
    /// </summary>
    public string? ContractReference { get; set; }

    /// <summary>
    /// Internal notes about this tenant-supplier relationship.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Start date of the supplier agreement.
    /// </summary>
    public DateTimeOffset? AgreementStartDate { get; set; }

    /// <summary>
    /// End date of the supplier agreement.
    /// </summary>
    public DateTimeOffset? AgreementEndDate { get; set; }
}
