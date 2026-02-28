namespace TadHub.SharedKernel.Events;

public sealed record DocumentCreatedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid WorkerDocumentId { get; init; }
    public Guid WorkerId { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string? ChangedByUserId { get; init; }
}

public sealed record DocumentUpdatedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid WorkerDocumentId { get; init; }
    public Guid WorkerId { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string? ChangedByUserId { get; init; }
}

public sealed record DocumentDeletedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid WorkerDocumentId { get; init; }
    public Guid WorkerId { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string? ChangedByUserId { get; init; }
}

public sealed record DocumentExpiringEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid WorkerDocumentId { get; init; }
    public Guid WorkerId { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public DateOnly ExpiresAt { get; init; }
    public int DaysRemaining { get; init; }
}

public sealed record DocumentExpiredEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid WorkerDocumentId { get; init; }
    public Guid WorkerId { get; init; }
    public string DocumentType { get; init; } = string.Empty;
}
