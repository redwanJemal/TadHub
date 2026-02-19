namespace SaasKit.Infrastructure.Settings;

public sealed class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";

    public int PermitLimit { get; init; } = 100;
    public int Window { get; init; } = 60;
    public int QueueLimit { get; init; } = 10;
}
