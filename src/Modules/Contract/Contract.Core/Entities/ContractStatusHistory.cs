using TadHub.SharedKernel.Entities;

namespace Contract.Core.Entities;

public class ContractStatusHistory : TenantScopedEntity
{
    public Guid ContractId { get; set; }
    public ContractStatus? FromStatus { get; set; }
    public ContractStatus ToStatus { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
    public Guid? ChangedBy { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Contract Contract { get; set; } = null!;
}
