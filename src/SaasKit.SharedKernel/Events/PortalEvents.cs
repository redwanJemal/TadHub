namespace SaasKit.SharedKernel.Events;

/// <summary>
/// Raised when a new portal is created.
/// </summary>
public sealed record PortalCreatedEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid TenantId,
    Guid PortalId,
    string Subdomain,
    string Name
) : IDomainEvent;

/// <summary>
/// Raised when a portal user registers.
/// </summary>
public sealed record PortalUserRegisteredEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid TenantId,
    Guid PortalId,
    Guid PortalUserId,
    string Email
) : IDomainEvent;

/// <summary>
/// Raised when a portal is published (goes live).
/// </summary>
public sealed record PortalPublishedEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid TenantId,
    Guid PortalId,
    string Subdomain
) : IDomainEvent;

/// <summary>
/// Raised when a portal's branding is updated.
/// </summary>
public sealed record PortalBrandingUpdatedEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid TenantId,
    Guid PortalId
) : IDomainEvent;
