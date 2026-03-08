using TadHub.SharedKernel.Entities;

namespace Runaway.Core.Entities;

public class RunawayCase : SoftDeletableEntity, IAuditable
{
    public string CaseCode { get; set; } = string.Empty;

    // Cross-module refs (GUIDs only, no EF FKs)
    public Guid WorkerId { get; set; }
    public Guid ContractId { get; set; }
    public Guid ClientId { get; set; }
    public Guid? SupplierId { get; set; }

    // Case info
    public RunawayCaseStatus Status { get; set; } = RunawayCaseStatus.Reported;
    public DateTimeOffset StatusChangedAt { get; set; }
    public DateTimeOffset ReportedDate { get; set; }
    public string ReportedBy { get; set; } = string.Empty;
    public string? LastKnownLocation { get; set; }
    public string? PoliceReportNumber { get; set; }
    public DateTimeOffset? PoliceReportDate { get; set; }

    // Guarantee
    public bool IsWithinGuarantee { get; set; }
    public GuaranteePeriodType? GuaranteePeriodType { get; set; }

    // Notes
    public string? Notes { get; set; }

    // Timestamps
    public DateTimeOffset? ConfirmedAt { get; set; }
    public DateTimeOffset? SettledAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }

    // IAuditable
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation
    public ICollection<RunawayExpense> Expenses { get; set; } = new List<RunawayExpense>();
    public ICollection<RunawayCaseStatusHistory> StatusHistory { get; set; } = new List<RunawayCaseStatusHistory>();
}
