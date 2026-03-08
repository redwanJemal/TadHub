namespace TadHub.SharedKernel.Events;

public sealed record TrialCreatedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid TrialId { get; init; }
    public Guid WorkerId { get; init; }
    public Guid ClientId { get; init; }
    public Guid? PlacementId { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }

    public TrialCreatedEvent() { }
}

public sealed record TrialCompletedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid TrialId { get; init; }
    public Guid WorkerId { get; init; }
    public Guid ClientId { get; init; }
    public Guid? PlacementId { get; init; }
    public string Outcome { get; init; } = string.Empty;
    public string? OutcomeNotes { get; init; }

    public TrialCompletedEvent() { }
}

public sealed record TrialCancelledEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid TrialId { get; init; }
    public Guid WorkerId { get; init; }
    public Guid ClientId { get; init; }
    public string? Reason { get; init; }

    public TrialCancelledEvent() { }
}
