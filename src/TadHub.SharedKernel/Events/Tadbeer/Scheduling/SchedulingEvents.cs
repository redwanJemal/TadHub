namespace TadHub.SharedKernel.Events.Tadbeer.Scheduling;

/// <summary>
/// Published when a booking is created for a flexible/temporary worker.
/// </summary>
public record BookingCreatedEvent : TadbeerEventBase
{
    public Guid BookingId { get; init; }
    public Guid ContractId { get; init; }
    public Guid WorkerId { get; init; }
    public Guid ClientId { get; init; }
    public string SlotType { get; init; } = string.Empty; // FourHour, EightHour, Daily, Weekly, Monthly
    public DateTimeOffset StartTime { get; init; }
    public DateTimeOffset EndTime { get; init; }
    public bool TransportRequired { get; init; }
}

/// <summary>
/// Published when a booking is cancelled.
/// </summary>
public record BookingCancelledEvent : TadbeerEventBase
{
    public Guid BookingId { get; init; }
    public Guid ContractId { get; init; }
    public Guid WorkerId { get; init; }
    public Guid ClientId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public Guid CancelledByUserId { get; init; }
    public DateTimeOffset OriginalStartTime { get; init; }
}

/// <summary>
/// Published when a client doesn't show up for a scheduled booking.
/// </summary>
public record NoShowRecordedEvent : TadbeerEventBase
{
    public Guid BookingId { get; init; }
    public Guid ContractId { get; init; }
    public Guid WorkerId { get; init; }
    public Guid ClientId { get; init; }
    public DateTimeOffset ScheduledStartTime { get; init; }
    public Guid RecordedByUserId { get; init; }
}
