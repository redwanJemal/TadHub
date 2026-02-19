namespace TadHub.SharedKernel.Events.Tadbeer.Pro;

/// <summary>
/// Published when a visa is issued for a worker.
/// </summary>
public record VisaIssuedEvent : TadbeerEventBase
{
    public Guid TransactionId { get; init; }
    public Guid WorkerId { get; init; }
    public Guid? ContractId { get; init; }
    public string VisaNumber { get; init; } = string.Empty;
    public DateTimeOffset ValidFrom { get; init; }
    public DateTimeOffset ValidUntil { get; init; }
}

/// <summary>
/// Published when a visa is about to expire.
/// </summary>
public record VisaExpiringEvent : TadbeerEventBase
{
    public Guid WorkerId { get; init; }
    public string VisaNumber { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
    public int DaysRemaining { get; init; }
}

/// <summary>
/// Published when a medical test is completed.
/// </summary>
public record MedicalTestCompletedEvent : TadbeerEventBase
{
    public Guid TransactionId { get; init; }
    public Guid WorkerId { get; init; }
    public string Result { get; init; } = string.Empty; // Fit, Unfit
    public DateTimeOffset ValidUntil { get; init; }
    public string? CertificateNumber { get; init; }
}

/// <summary>
/// Published when an Emirates ID is issued.
/// </summary>
public record EmiratesIdIssuedEvent : TadbeerEventBase
{
    public Guid TransactionId { get; init; }
    public Guid WorkerId { get; init; }
    public string EmiratesIdNumber { get; init; } = string.Empty;
    public DateTimeOffset ValidUntil { get; init; }
}

/// <summary>
/// Published when health insurance is activated.
/// </summary>
public record InsuranceActivatedEvent : TadbeerEventBase
{
    public Guid TransactionId { get; init; }
    public Guid WorkerId { get; init; }
    public Guid? ContractId { get; init; }
    public string PolicyNumber { get; init; } = string.Empty;
    public string Provider { get; init; } = string.Empty;
    public DateTimeOffset ValidFrom { get; init; }
    public DateTimeOffset ValidUntil { get; init; }
}

/// <summary>
/// Published when a document is about to expire.
/// </summary>
public record DocumentExpiringEvent : TadbeerEventBase
{
    public Guid WorkerId { get; init; }
    public string DocumentType { get; init; } = string.Empty; // Passport, EmiratesId, MedicalCertificate, etc.
    public string DocumentNumber { get; init; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; init; }
    public int DaysRemaining { get; init; }
}
