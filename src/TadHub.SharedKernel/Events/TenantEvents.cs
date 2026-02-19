namespace TadHub.SharedKernel.Events;

/// <summary>
/// Raised when a new tenant is created.
/// </summary>
public sealed record TenantCreatedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public Guid CreatedByUserId { get; init; }

    public TenantCreatedEvent() { }

    public TenantCreatedEvent(Guid tenantId, string name, string slug, Guid createdByUserId, DateTimeOffset occurredAt)
    {
        TenantId = tenantId;
        Name = name;
        Slug = slug;
        CreatedByUserId = createdByUserId;
        OccurredAt = occurredAt;
    }
}

/// <summary>
/// Raised when a tenant is updated.
/// </summary>
public sealed record TenantUpdatedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;

    public TenantUpdatedEvent() { }

    public TenantUpdatedEvent(Guid tenantId, string name, DateTimeOffset occurredAt)
    {
        TenantId = tenantId;
        Name = name;
        OccurredAt = occurredAt;
    }
}

/// <summary>
/// Raised when a tenant is suspended.
/// </summary>
public sealed record TenantSuspendedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }

    public TenantSuspendedEvent() { }

    public TenantSuspendedEvent(Guid tenantId, DateTimeOffset occurredAt)
    {
        TenantId = tenantId;
        OccurredAt = occurredAt;
    }
}

/// <summary>
/// Raised when a tenant is deleted (soft delete).
/// </summary>
public sealed record TenantDeletedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }

    public TenantDeletedEvent() { }

    public TenantDeletedEvent(Guid tenantId, DateTimeOffset occurredAt)
    {
        TenantId = tenantId;
        OccurredAt = occurredAt;
    }
}

/// <summary>
/// Raised when a user is invited to a tenant.
/// </summary>
public sealed record TenantUserInvitedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid InvitationId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public Guid InvitedByUserId { get; init; }

    public TenantUserInvitedEvent() { }
}

/// <summary>
/// Raised when a user accepts a tenant invitation.
/// </summary>
public sealed record TenantInvitationAcceptedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid InvitationId { get; init; }
    public Guid UserId { get; init; }

    public TenantInvitationAcceptedEvent() { }
}

/// <summary>
/// Raised when a user is removed from a tenant.
/// </summary>
public sealed record TenantUserRemovedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid UserId { get; init; }
    public Guid RemovedByUserId { get; init; }

    public TenantUserRemovedEvent() { }
}
