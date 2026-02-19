namespace TadHub.SharedKernel.Interfaces;

/// <summary>
/// Abstraction over system clock for testability.
/// Use IClock.UtcNow instead of DateTime.UtcNow or DateTimeOffset.UtcNow.
/// In tests, use FakeClock to control time.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    DateTimeOffset UtcNow { get; }

    /// <summary>
    /// Gets the current UTC date.
    /// </summary>
    DateOnly Today => DateOnly.FromDateTime(UtcNow.DateTime);
}
