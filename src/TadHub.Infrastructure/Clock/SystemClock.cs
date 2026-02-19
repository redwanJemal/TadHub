using TadHub.SharedKernel.Interfaces;

namespace TadHub.Infrastructure.Clock;

/// <summary>
/// Production implementation of IClock that uses the system clock.
/// Register as singleton in DI.
/// </summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
