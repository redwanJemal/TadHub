namespace TadHub.SharedKernel.Events;

/// <summary>
/// Raised when a contract status is transitioned.
/// The Worker module consumes this to sync worker inventory status.
/// </summary>
public sealed record ContractStatusChangedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid ContractId { get; init; }
    public Guid WorkerId { get; init; }
    public string FromStatus { get; init; } = string.Empty;
    public string ToStatus { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public string? TerminationReason { get; init; }
    public string? ChangedByUserId { get; init; }
    public Guid? ClientId { get; init; }

    public ContractStatusChangedEvent() { }
}

/// <summary>
/// Raised when a contract is auto-created (e.g., from a successful trial).
/// </summary>
public sealed record ContractAutoCreatedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid ContractId { get; init; }
    public Guid WorkerId { get; init; }
    public Guid ClientId { get; init; }
    public string ContractCode { get; init; } = string.Empty;
    public string ContractType { get; init; } = string.Empty;
    public Guid? TrialId { get; init; }
    public Guid? PlacementId { get; init; }

    public ContractAutoCreatedEvent() { }
}
