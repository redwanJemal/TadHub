namespace TadHub.SharedKernel.Events.Tadbeer;

/// <summary>
/// Base record for all Tadbeer domain events.
/// All events include correlation tracking for distributed tracing.
/// </summary>
public abstract record TadbeerEventBase : IDomainEvent
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// When the event occurred.
    /// </summary>
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// The tenant this event belongs to.
    /// </summary>
    public Guid TenantId { get; init; }

    /// <summary>
    /// Correlation ID for tracing a business process across events.
    /// </summary>
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
}
