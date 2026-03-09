namespace TadHub.SharedKernel.Events;

public sealed record ArrivalScheduledEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid ArrivalId { get; init; }
    public Guid PlacementId { get; init; }
    public Guid WorkerId { get; init; }
    public DateOnly ScheduledArrivalDate { get; init; }
    public string? FlightNumber { get; init; }

    public ArrivalScheduledEvent() { }
}

public sealed record ArrivalConfirmedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid ArrivalId { get; init; }
    public Guid PlacementId { get; init; }
    public Guid WorkerId { get; init; }
    public DateTimeOffset ActualArrivalTime { get; init; }

    public ArrivalConfirmedEvent() { }
}

public sealed record MaidNoShowEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid ArrivalId { get; init; }
    public Guid PlacementId { get; init; }
    public Guid WorkerId { get; init; }
    public Guid? SupplierId { get; init; }
    public DateOnly ScheduledArrivalDate { get; init; }

    public MaidNoShowEvent() { }
}

public sealed record MaidAtAccommodationEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid ArrivalId { get; init; }
    public Guid PlacementId { get; init; }
    public Guid WorkerId { get; init; }

    public MaidAtAccommodationEvent() { }
}
