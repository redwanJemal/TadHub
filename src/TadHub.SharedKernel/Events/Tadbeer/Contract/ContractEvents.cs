namespace TadHub.SharedKernel.Events.Tadbeer.Contract;

/// <summary>
/// Published when a new contract is created (in Draft status).
/// </summary>
public record ContractCreatedEvent : TadbeerEventBase
{
    public Guid ContractId { get; init; }
    public string ContractNumber { get; init; } = string.Empty;
    public string ContractType { get; init; } = string.Empty; // Traditional, Temporary, Flexible
    public Guid ClientId { get; init; }
    public Guid WorkerId { get; init; }
    public decimal TotalAmount { get; init; }
    public Guid CreatedByUserId { get; init; }
}

/// <summary>
/// Published when a contract is activated (all preconditions met).
/// Worker transitions to Hired status.
/// </summary>
public record ContractActivatedEvent : TadbeerEventBase
{
    public Guid ContractId { get; init; }
    public string ContractType { get; init; } = string.Empty;
    public Guid ClientId { get; init; }
    public Guid WorkerId { get; init; }
    public DateTimeOffset StartDate { get; init; }
    public DateTimeOffset EndDate { get; init; }
    public int GuaranteePeriodDays { get; init; }
    public Guid ApprovedByUserId { get; init; }
}

/// <summary>
/// Published when a contract is terminated.
/// </summary>
public record ContractTerminatedEvent : TadbeerEventBase
{
    public Guid ContractId { get; init; }
    public Guid ClientId { get; init; }
    public Guid WorkerId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string InitiatedBy { get; init; } = string.Empty; // Client, Agency, System
    public bool RefundEligible { get; init; }
    public decimal? RefundAmount { get; init; }
}

/// <summary>
/// Published when a contract is renewed.
/// </summary>
public record ContractRenewedEvent : TadbeerEventBase
{
    public Guid OriginalContractId { get; init; }
    public Guid NewContractId { get; init; }
    public Guid ClientId { get; init; }
    public Guid WorkerId { get; init; }
    public DateTimeOffset NewEndDate { get; init; }
    public decimal PriceAdjustment { get; init; }
}

/// <summary>
/// Published when a guarantee period is about to expire.
/// Published at 30, 14, and 7 days before expiry.
/// </summary>
public record GuaranteePeriodExpiringEvent : TadbeerEventBase
{
    public Guid ContractId { get; init; }
    public Guid ClientId { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
    public int DaysRemaining { get; init; }
}

/// <summary>
/// Published when a worker replacement is requested within guarantee period.
/// </summary>
public record ReplacementRequestedEvent : TadbeerEventBase
{
    public Guid ContractId { get; init; }
    public Guid OriginalWorkerId { get; init; }
    public string Reason { get; init; } = string.Empty; // Incompetence, Absconding, MedicalUnfitness, AgencyNonCompliance
    public Guid RequestedByUserId { get; init; }
}

/// <summary>
/// Published when a refund is triggered.
/// Financial module picks this up to process the refund.
/// </summary>
public record RefundTriggeredEvent : TadbeerEventBase
{
    public Guid ContractId { get; init; }
    public Guid ClientId { get; init; }
    public string ReasonCode { get; init; } = string.Empty;
    public decimal RequestedAmount { get; init; }
    public DateTimeOffset Deadline { get; init; } // 14 days from trigger
}

/// <summary>
/// Published when sponsorship is transferred (for temporary contracts).
/// </summary>
public record SponsorshipTransferEvent : TadbeerEventBase
{
    public Guid ContractId { get; init; }
    public Guid WorkerId { get; init; }
    public Guid FromSponsorId { get; init; } // Agency tenant
    public Guid ToSponsorId { get; init; } // Client/employer
    public decimal TransferFee { get; init; }
}
