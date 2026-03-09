namespace TadHub.SharedKernel.Events;

public sealed record PlacementCreatedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid PlacementId { get; init; }
    public Guid CandidateId { get; init; }
    public Guid ClientId { get; init; }
    public Guid? BookedBy { get; init; }
    public string FlowType { get; init; } = "OutsideCountry";

    public PlacementCreatedEvent() { }
}

public sealed record PlacementStatusChangedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid PlacementId { get; init; }
    public Guid CandidateId { get; init; }
    public Guid? WorkerId { get; init; }
    public string FromStatus { get; init; } = string.Empty;
    public string ToStatus { get; init; } = string.Empty;
    public string? Reason { get; init; }

    public PlacementStatusChangedEvent() { }
}
