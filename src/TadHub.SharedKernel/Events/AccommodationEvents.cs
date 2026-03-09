namespace TadHub.SharedKernel.Events;

public sealed record AccommodationCheckInEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid StayId { get; init; }
    public Guid WorkerId { get; init; }
    public Guid? PlacementId { get; init; }
    public Guid? ArrivalId { get; init; }
    public string? Room { get; init; }
    public string? Location { get; init; }

    public AccommodationCheckInEvent() { }
}

public sealed record AccommodationCheckOutEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid StayId { get; init; }
    public Guid WorkerId { get; init; }
    public string DepartureReason { get; init; } = string.Empty;

    public AccommodationCheckOutEvent() { }
}
