using TadHub.SharedKernel.Entities;

namespace Placement.Core.Entities;

public class Placement : SoftDeletableEntity, IAuditable
{
    public string PlacementCode { get; set; } = string.Empty;
    public PlacementStatus Status { get; set; } = PlacementStatus.Booked;
    public DateTimeOffset StatusChangedAt { get; set; }
    public string? StatusReason { get; set; }
    public PlacementFlowType FlowType { get; set; } = PlacementFlowType.OutsideCountry;

    // Cross-module refs (GUIDs only, no EF FKs)
    public Guid CandidateId { get; set; }
    public Guid ClientId { get; set; }
    public Guid? WorkerId { get; set; }
    public Guid? ContractId { get; set; }
    public Guid? TrialId { get; set; }

    // Booking info
    public Guid? BookedBy { get; set; }
    public string? BookedByName { get; set; }
    public DateTimeOffset BookedAt { get; set; }
    public string? BookingNotes { get; set; }

    // Pipeline dates
    public DateTimeOffset? ContractCreatedAt { get; set; }
    public DateTimeOffset? EmploymentVisaStartedAt { get; set; }
    public DateOnly? TicketDate { get; set; }
    public string? FlightDetails { get; set; }
    public DateOnly? ExpectedArrivalDate { get; set; }
    public DateTimeOffset? ArrivedAt { get; set; }
    public DateTimeOffset? DeployedAt { get; set; }
    public DateTimeOffset? FullPaymentReceivedAt { get; set; }
    public DateTimeOffset? ResidenceVisaStartedAt { get; set; }
    public DateTimeOffset? EmiratesIdStartedAt { get; set; }
    public DateTimeOffset? TrialStartedAt { get; set; }
    public DateTimeOffset? TrialSucceededAt { get; set; }
    public DateTimeOffset? StatusChangedStepAt { get; set; }
    public DateTimeOffset? MedicalClearedAt { get; set; }
    public DateTimeOffset? GovtClearedAt { get; set; }
    public DateTimeOffset? PlacedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    // Visa cross-module refs
    public Guid? EmploymentVisaApplicationId { get; set; }
    public Guid? ResidenceVisaApplicationId { get; set; }
    public Guid? EmiratesIdApplicationId { get; set; }

    // Arrival cross-module ref
    public Guid? ArrivalId { get; set; }

    // Currency for costs
    public string Currency { get; set; } = "AED";

    // Audit
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation
    public ICollection<PlacementCostItem> CostItems { get; set; } = new List<PlacementCostItem>();
    public ICollection<PlacementStatusHistory> StatusHistory { get; set; } = new List<PlacementStatusHistory>();
}
