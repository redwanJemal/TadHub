using TadHub.SharedKernel.Entities;

namespace Contract.Core.Entities;

public class Contract : SoftDeletableEntity, IAuditable
{
    public string ContractCode { get; set; } = string.Empty;

    // Type & Status
    public ContractType Type { get; set; } = ContractType.Traditional;
    public ContractStatus Status { get; set; } = ContractStatus.Draft;
    public DateTimeOffset? StatusChangedAt { get; set; }
    public string? StatusReason { get; set; }

    // Parties (cross-module GUIDs, no EF FK)
    public Guid WorkerId { get; set; }
    public Guid ClientId { get; set; }

    // Dates
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public DateOnly? ProbationEndDate { get; set; }
    public DateOnly? GuaranteeEndDate { get; set; }
    public bool ProbationPassed { get; set; }

    // Financial
    public decimal Rate { get; set; }
    public RatePeriod RatePeriod { get; set; } = RatePeriod.Monthly;
    public string Currency { get; set; } = "AED";
    public decimal? TotalValue { get; set; }

    // Termination
    public DateTimeOffset? TerminatedAt { get; set; }
    public string? TerminationReason { get; set; }
    public TerminatedByParty? TerminatedBy { get; set; }

    // Replacement linkage
    public Guid? ReplacementContractId { get; set; }
    public Guid? OriginalContractId { get; set; }

    public string? Notes { get; set; }

    // IAuditable
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation
    public ICollection<ContractStatusHistory> StatusHistory { get; set; } = new List<ContractStatusHistory>();
}
