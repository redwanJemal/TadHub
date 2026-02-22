namespace Supplier.Contracts.DTOs;

/// <summary>
/// Tenant-supplier relationship data transfer object.
/// </summary>
public sealed record TenantSupplierDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid SupplierId { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? ContractReference { get; init; }
    public string? Notes { get; init; }
    public DateTimeOffset? AgreementStartDate { get; init; }
    public DateTimeOffset? AgreementEndDate { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    /// <summary>
    /// The supplier details. Included when requested via ?include=supplier.
    /// </summary>
    public SupplierDto? Supplier { get; init; }
}
