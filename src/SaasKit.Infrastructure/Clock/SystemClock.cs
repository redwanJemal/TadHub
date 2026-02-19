using SaasKit.SharedKernel.Interfaces;

namespace SaasKit.Infrastructure.Clock;

/// <summary>
/// Production implementation of IClock that uses the system clock.
/// Register as singleton in DI.
/// </summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
