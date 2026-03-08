namespace TadHub.SharedKernel.Events;

public sealed record ReturneeCaseCreatedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid ReturneeCaseId { get; init; }
    public Guid WorkerId { get; init; }
    public Guid ContractId { get; init; }
    public Guid ClientId { get; init; }
    public Guid? SupplierId { get; init; }
    public string ReturnType { get; init; } = string.Empty;
    public string ReturnReason { get; init; } = string.Empty;
}

public sealed record ReturneeCaseApprovedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid ReturneeCaseId { get; init; }
    public Guid WorkerId { get; init; }
    public Guid ContractId { get; init; }
    public Guid ClientId { get; init; }
    public Guid? SupplierId { get; init; }
    public string ReturnType { get; init; } = string.Empty;
    public bool IsWithinGuarantee { get; init; }
    public decimal? RefundAmount { get; init; }
    public int MonthsWorked { get; init; }
}

public sealed record ReturneeCaseSettledEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid ReturneeCaseId { get; init; }
    public Guid WorkerId { get; init; }
    public Guid ContractId { get; init; }
}
