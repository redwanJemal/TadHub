namespace SaasKit.SharedKernel.Events;

/// <summary>
/// Base interface for all domain events.
/// Domain events are immutable records that represent something that happened in the domain.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    DateTimeOffset OccurredAt { get; }
}
