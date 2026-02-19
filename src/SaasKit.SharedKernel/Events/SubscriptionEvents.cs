namespace SaasKit.SharedKernel.Events;

/// <summary>
/// Raised when a new subscription is created.
/// </summary>
public sealed record SubscriptionCreatedEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid TenantId,
    Guid SubscriptionId,
    string PlanId,
    string PlanName
) : IDomainEvent;

/// <summary>
/// Raised when a subscription plan is changed (upgrade/downgrade).
/// </summary>
public sealed record SubscriptionChangedEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid TenantId,
    Guid SubscriptionId,
    string OldPlanId,
    string NewPlanId,
    string ChangeReason
) : IDomainEvent;

/// <summary>
/// Raised when a subscription is cancelled.
/// </summary>
public sealed record SubscriptionCancelledEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid TenantId,
    Guid SubscriptionId,
    string Reason,
    DateTimeOffset EffectiveAt
) : IDomainEvent;

/// <summary>
/// Raised when a subscription is renewed.
/// </summary>
public sealed record SubscriptionRenewedEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid TenantId,
    Guid SubscriptionId,
    DateTimeOffset NewPeriodEnd
) : IDomainEvent;

/// <summary>
/// Raised when a subscription payment fails.
/// </summary>
public sealed record SubscriptionPaymentFailedEvent(
    Guid EventId,
    DateTimeOffset OccurredAt,
    Guid TenantId,
    Guid SubscriptionId,
    string FailureReason,
    int AttemptCount
) : IDomainEvent;
