using TadHub.SharedKernel.Entities;

namespace Returnee.Core.Entities;

public class ReturneeCase : SoftDeletableEntity, IAuditable
{
    public string CaseCode { get; set; } = string.Empty;

    // Cross-module refs (GUIDs only, no EF FKs)
    public Guid WorkerId { get; set; }
    public Guid ContractId { get; set; }
    public Guid ClientId { get; set; }
    public Guid? SupplierId { get; set; }

    // Case info
    public ReturnType ReturnType { get; set; }
    public ReturneeCaseStatus Status { get; set; } = ReturneeCaseStatus.Submitted;
    public DateTimeOffset StatusChangedAt { get; set; }
    public DateOnly ReturnDate { get; set; }
    public string ReturnReason { get; set; } = string.Empty;

    // Calculated fields
    public int MonthsWorked { get; set; }
    public bool IsWithinGuarantee { get; set; }
    public GuaranteePeriodType? GuaranteePeriodType { get; set; }

    // Refund
    public decimal? TotalAmountPaid { get; set; }
    public decimal? RefundAmount { get; set; }
    public string Currency { get; set; } = "AED";

    // Approval / Rejection
    public string? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public string? RejectedReason { get; set; }

    // Settlement
    public DateTimeOffset? SettledAt { get; set; }
    public string? SettlementNotes { get; set; }

    public string? Notes { get; set; }

    // IAuditable
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation
    public ICollection<ReturneeExpense> Expenses { get; set; } = new List<ReturneeExpense>();
    public ICollection<ReturneeCaseStatusHistory> StatusHistory { get; set; } = new List<ReturneeCaseStatusHistory>();
}
