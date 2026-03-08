namespace TadHub.SharedKernel.Events;

public sealed record VisaApplicationCreatedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid VisaApplicationId { get; init; }
    public Guid WorkerId { get; init; }
    public Guid ClientId { get; init; }
    public string VisaType { get; init; } = string.Empty;
    public Guid? PlacementId { get; init; }
    public Guid? CreatedBy { get; init; }

    public VisaApplicationCreatedEvent() { }
}

public sealed record VisaStatusChangedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid VisaApplicationId { get; init; }
    public Guid WorkerId { get; init; }
    public string VisaType { get; init; } = string.Empty;
    public string FromStatus { get; init; } = string.Empty;
    public string ToStatus { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public Guid? PlacementId { get; init; }

    public VisaStatusChangedEvent() { }
}

public sealed record VisaIssuedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid VisaApplicationId { get; init; }
    public Guid WorkerId { get; init; }
    public string VisaType { get; init; } = string.Empty;
    public string? VisaNumber { get; init; }
    public DateOnly? ExpiryDate { get; init; }
    public Guid? PlacementId { get; init; }

    public VisaIssuedEvent() { }
}
