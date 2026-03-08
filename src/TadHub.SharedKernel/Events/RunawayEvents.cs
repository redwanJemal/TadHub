namespace TadHub.SharedKernel.Events;

public sealed record RunawayCaseReportedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid RunawayCaseId { get; init; }
    public Guid WorkerId { get; init; }
    public Guid ContractId { get; init; }
    public Guid ClientId { get; init; }
    public Guid? SupplierId { get; init; }
    public bool IsWithinGuarantee { get; init; }
}

public sealed record RunawayCaseConfirmedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid RunawayCaseId { get; init; }
    public Guid WorkerId { get; init; }
    public Guid ContractId { get; init; }
    public Guid ClientId { get; init; }
    public Guid? SupplierId { get; init; }
    public bool IsWithinGuarantee { get; init; }
}
