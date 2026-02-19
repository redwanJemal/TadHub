namespace TadHub.SharedKernel.Events.Tadbeer.Worker;

/// <summary>
/// Published when a new worker is created.
/// </summary>
public record WorkerCreatedEvent : TadbeerEventBase
{
    public Guid WorkerId { get; init; }
    public string CvSerial { get; init; } = string.Empty;
    public string FullNameEn { get; init; } = string.Empty;
    public string Nationality { get; init; } = string.Empty;
    public Guid JobCategoryId { get; init; }
    public Guid? CreatedByUserId { get; init; }
}

/// <summary>
/// Published when a worker's status changes in the 20-state machine.
/// </summary>
public record WorkerStatusChangedEvent : TadbeerEventBase
{
    public Guid WorkerId { get; init; }
    public string FromStatus { get; init; } = string.Empty;
    public string ToStatus { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public Guid? ChangedByUserId { get; init; }
}

/// <summary>
/// Published when a worker absconds (leaves without notice).
/// Triggers contract termination, refund, and visa cancellation.
/// </summary>
public record WorkerAbscondedEvent : TadbeerEventBase
{
    public Guid WorkerId { get; init; }
    public Guid? ActiveContractId { get; init; }
    public string? Notes { get; init; }
    public Guid ReportedByUserId { get; init; }
}

/// <summary>
/// Published when a worker's medical test results are received.
/// </summary>
public record WorkerMedicalResultEvent : TadbeerEventBase
{
    public Guid WorkerId { get; init; }
    public string Result { get; init; } = string.Empty; // Cleared, Unfit
    public string? Details { get; init; }
    public DateTimeOffset TestDate { get; init; }
}

/// <summary>
/// Published when a worker is booked for a contract.
/// Worker transitions from ReadyForMarket to Booked.
/// </summary>
public record WorkerBookedEvent : TadbeerEventBase
{
    public Guid WorkerId { get; init; }
    public Guid ContractId { get; init; }
    public Guid ClientId { get; init; }
    public string ContractType { get; init; } = string.Empty;
}
