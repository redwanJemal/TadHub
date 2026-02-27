namespace TadHub.SharedKernel.Events;

public sealed record ClientCreatedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid ClientId { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string? ChangedByUserId { get; init; }
}

public sealed record ClientUpdatedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid ClientId { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string? ChangedByUserId { get; init; }
}

public sealed record ClientDeletedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid ClientId { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string? ChangedByUserId { get; init; }
}
