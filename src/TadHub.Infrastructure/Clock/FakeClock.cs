using TadHub.SharedKernel.Interfaces;

namespace TadHub.Infrastructure.Clock;

/// <summary>
/// Test implementation of IClock that allows controlling time.
/// Use in unit and integration tests to make time-dependent code deterministic.
/// </summary>
public sealed class FakeClock : IClock
{
    private DateTimeOffset _now;

    /// <summary>
    /// Creates a FakeClock set to the specified time, or current UTC time if not provided.
    /// </summary>
    public FakeClock(DateTimeOffset? fixedTime = null)
    {
        _now = fixedTime ?? DateTimeOffset.UtcNow;
    }

    /// <inheritdoc />
    public DateTimeOffset UtcNow => _now;

    /// <summary>
    /// Advances the clock by the specified duration.
    /// </summary>
    public void Advance(TimeSpan duration)
    {
        _now = _now.Add(duration);
    }

    /// <summary>
    /// Sets the clock to a specific time.
    /// </summary>
    public void SetTime(DateTimeOffset time)
    {
        _now = time;
    }

    /// <summary>
    /// Resets the clock to the current UTC time.
    /// </summary>
    public void Reset()
    {
        _now = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Creates a FakeClock set to a specific date (midnight UTC).
    /// </summary>
    public static FakeClock AtDate(int year, int month, int day) =>
        new(new DateTimeOffset(year, month, day, 0, 0, 0, TimeSpan.Zero));

    /// <summary>
    /// Creates a FakeClock set to a specific date and time.
    /// </summary>
    public static FakeClock At(int year, int month, int day, int hour = 0, int minute = 0, int second = 0) =>
        new(new DateTimeOffset(year, month, day, hour, minute, second, TimeSpan.Zero));
}
