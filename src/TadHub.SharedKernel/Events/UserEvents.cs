namespace TadHub.SharedKernel.Events;

/// <summary>
/// Raised when a new user is created (synced from Keycloak).
/// </summary>
public sealed record UserCreatedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid UserId { get; init; }
    public string KeycloakId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;

    public UserCreatedEvent() { }

    public UserCreatedEvent(
        Guid userId,
        string keycloakId,
        string email,
        string firstName,
        string lastName,
        DateTimeOffset occurredAt)
    {
        UserId = userId;
        KeycloakId = keycloakId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        OccurredAt = occurredAt;
    }
}

/// <summary>
/// Raised when a user profile is updated.
/// </summary>
public sealed record UserUpdatedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;

    public UserUpdatedEvent() { }

    public UserUpdatedEvent(
        Guid userId,
        string email,
        string firstName,
        string lastName,
        DateTimeOffset occurredAt)
    {
        UserId = userId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        OccurredAt = occurredAt;
    }
}

/// <summary>
/// Raised when a user is deactivated.
/// </summary>
public sealed record UserDeactivatedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid UserId { get; init; }
    public Guid? DeactivatedByUserId { get; init; }
    public string? Reason { get; init; }

    public UserDeactivatedEvent() { }

    public UserDeactivatedEvent(Guid userId, DateTimeOffset occurredAt)
    {
        UserId = userId;
        OccurredAt = occurredAt;
    }
}

/// <summary>
/// Raised when a user logs in.
/// </summary>
public sealed record UserLoggedInEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid UserId { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }

    public UserLoggedInEvent() { }

    public UserLoggedInEvent(Guid userId, DateTimeOffset occurredAt)
    {
        UserId = userId;
        OccurredAt = occurredAt;
    }
}
