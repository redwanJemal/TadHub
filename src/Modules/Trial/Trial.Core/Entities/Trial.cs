using TadHub.SharedKernel.Entities;

namespace Trial.Core.Entities;

public class Trial : SoftDeletableEntity, IAuditable
{
    public string TrialCode { get; set; } = string.Empty;
    public TrialStatus Status { get; set; } = TrialStatus.Active;
    public DateTimeOffset StatusChangedAt { get; set; }

    // Cross-module refs (GUIDs only, no EF FKs)
    public Guid WorkerId { get; set; }
    public Guid ClientId { get; set; }
    public Guid? PlacementId { get; set; }
    public Guid? ContractId { get; set; }

    // Trial period
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    // Outcome
    public TrialOutcome? Outcome { get; set; }
    public string? OutcomeNotes { get; set; }
    public DateTimeOffset? OutcomeDate { get; set; }

    // Audit
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation
    public ICollection<TrialStatusHistory> StatusHistory { get; set; } = new List<TrialStatusHistory>();
}
