namespace Placement.Contracts.DTOs;

public sealed record PlacementDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string PlacementCode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset StatusChangedAt { get; init; }
    public string? StatusReason { get; init; }

    // Cross-module refs
    public Guid CandidateId { get; init; }
    public Guid ClientId { get; init; }
    public Guid? WorkerId { get; init; }
    public Guid? ContractId { get; init; }

    // Visa cross-module refs
    public Guid? EmploymentVisaApplicationId { get; init; }
    public Guid? ResidenceVisaApplicationId { get; init; }
    public Guid? EmiratesIdApplicationId { get; init; }

    // Arrival ref
    public Guid? ArrivalId { get; init; }

    // BFF-enriched
    public PlacementCandidateDto? Candidate { get; init; }
    public PlacementClientDto? Client { get; init; }
    public PlacementWorkerDto? Worker { get; init; }

    // Booking
    public Guid? BookedBy { get; init; }
    public string? BookedByName { get; init; }
    public DateTimeOffset BookedAt { get; init; }
    public string? BookingNotes { get; init; }

    // Pipeline dates
    public DateTimeOffset? ContractCreatedAt { get; init; }
    public DateTimeOffset? EmploymentVisaStartedAt { get; init; }
    public DateOnly? TicketDate { get; init; }
    public string? FlightDetails { get; init; }
    public DateOnly? ExpectedArrivalDate { get; init; }
    public DateTimeOffset? ArrivedAt { get; init; }
    public DateTimeOffset? DeployedAt { get; init; }
    public DateTimeOffset? FullPaymentReceivedAt { get; init; }
    public DateTimeOffset? ResidenceVisaStartedAt { get; init; }
    public DateTimeOffset? EmiratesIdStartedAt { get; init; }
    public DateTimeOffset? MedicalClearedAt { get; init; }
    public DateTimeOffset? GovtClearedAt { get; init; }
    public DateTimeOffset? PlacedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public DateTimeOffset? CancelledAt { get; init; }
    public string? CancellationReason { get; init; }

    // Currency
    public string Currency { get; init; } = "AED";

    // Audit
    public Guid? CreatedBy { get; init; }
    public Guid? UpdatedBy { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    // Optional includes
    public List<PlacementCostItemDto>? CostItems { get; init; }
    public List<PlacementStatusHistoryDto>? StatusHistory { get; init; }
    public decimal? TotalCost { get; init; }

    // Checklist (optional include)
    public PlacementChecklistDto? Checklist { get; init; }
}

public sealed record PlacementListDto
{
    public Guid Id { get; init; }
    public string PlacementCode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset StatusChangedAt { get; init; }
    public Guid CandidateId { get; init; }
    public Guid ClientId { get; init; }
    public Guid? WorkerId { get; init; }
    public Guid? ContractId { get; init; }
    public PlacementCandidateDto? Candidate { get; init; }
    public PlacementClientDto? Client { get; init; }
    public DateTimeOffset BookedAt { get; init; }
    public DateOnly? ExpectedArrivalDate { get; init; }
    public decimal TotalCost { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public int CurrentStep { get; init; }
    public int TotalSteps { get; init; }
}

public sealed record PlacementCandidateDto
{
    public Guid Id { get; init; }
    public string FullNameEn { get; init; } = string.Empty;
    public string? FullNameAr { get; init; }
    public string? Nationality { get; init; }
    public string? PhotoUrl { get; init; }
}

public sealed record PlacementClientDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string? NameAr { get; init; }
}

public sealed record PlacementWorkerDto
{
    public Guid Id { get; init; }
    public string FullNameEn { get; init; } = string.Empty;
    public string? FullNameAr { get; init; }
    public string WorkerCode { get; init; } = string.Empty;
}

public sealed record PlacementCostItemDto
{
    public Guid Id { get; init; }
    public Guid PlacementId { get; init; }
    public string CostType { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "AED";
    public string Status { get; init; } = string.Empty;
    public DateOnly? CostDate { get; init; }
    public DateTimeOffset? PaidAt { get; init; }
    public string? ReferenceNumber { get; init; }
    public string? Notes { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record PlacementStatusHistoryDto
{
    public Guid Id { get; init; }
    public Guid PlacementId { get; init; }
    public string? FromStatus { get; init; }
    public string ToStatus { get; init; } = string.Empty;
    public DateTimeOffset ChangedAt { get; init; }
    public string? ChangedBy { get; init; }
    public string? Reason { get; init; }
    public string? Notes { get; init; }
}

public sealed record CreatePlacementRequest
{
    public Guid CandidateId { get; init; }
    public Guid ClientId { get; init; }
    public string? BookingNotes { get; init; }
    public List<CreatePlacementCostItemRequest>? InitialCostItems { get; init; }
}

public sealed record UpdatePlacementRequest
{
    public DateOnly? TicketDate { get; init; }
    public string? FlightDetails { get; init; }
    public DateOnly? ExpectedArrivalDate { get; init; }
    public string? BookingNotes { get; init; }
    public Guid? ContractId { get; init; }
    public Guid? EmploymentVisaApplicationId { get; init; }
    public Guid? ResidenceVisaApplicationId { get; init; }
    public Guid? EmiratesIdApplicationId { get; init; }
    public Guid? ArrivalId { get; init; }
}

public sealed record TransitionPlacementStatusRequest
{
    public string Status { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public string? Notes { get; init; }
}

public sealed record AdvancePlacementStepRequest
{
    public string? Notes { get; init; }
}

public sealed record CreatePlacementCostItemRequest
{
    public string CostType { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "AED";
    public string Status { get; init; } = "Pending";
    public DateOnly? CostDate { get; init; }
    public string? ReferenceNumber { get; init; }
    public string? Notes { get; init; }
}

public sealed record UpdatePlacementCostItemRequest
{
    public string? CostType { get; init; }
    public string? Description { get; init; }
    public decimal? Amount { get; init; }
    public string? Currency { get; init; }
    public string? Status { get; init; }
    public DateOnly? CostDate { get; init; }
    public DateTimeOffset? PaidAt { get; init; }
    public string? ReferenceNumber { get; init; }
    public string? Notes { get; init; }
}

public sealed record PlacementBoardDto
{
    public Dictionary<string, int> StatusCounts { get; init; } = new();
    public Dictionary<string, List<PlacementListDto>> Columns { get; init; } = new();
}

// Checklist DTOs for 9-step progress tracking
public sealed record PlacementChecklistDto
{
    public List<PlacementChecklistStepDto> Steps { get; init; } = new();
    public int CurrentStepNumber { get; init; }
    public int TotalSteps { get; init; }
    public double ProgressPercent { get; init; }
}

public sealed record PlacementChecklistStepDto
{
    public int StepNumber { get; init; }
    public string Status { get; init; } = string.Empty;
    public string StepStatus { get; init; } = string.Empty; // Pending, InProgress, Completed
    public string Label { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTimeOffset? CompletedAt { get; init; }
    public string? ActionLabel { get; init; }
    public Guid? LinkedEntityId { get; init; }
    public string? LinkedEntityType { get; init; }
}
