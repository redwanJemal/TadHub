namespace TadHub.Infrastructure.Settings;

public sealed class FeatureSettings
{
    public const string SectionName = "Features";

    public bool EnableSwagger { get; init; } = true;
    public bool EnableHangfireDashboard { get; init; } = true;
    public bool EnableHealthChecks { get; init; } = true;
    public bool EnableDetailedErrors { get; init; } = false;
}
