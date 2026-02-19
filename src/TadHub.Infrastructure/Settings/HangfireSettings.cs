namespace TadHub.Infrastructure.Settings;

public sealed class HangfireSettings
{
    public const string SectionName = "Hangfire";

    public string DashboardPath { get; init; } = "/hangfire";
    public int WorkerCount { get; init; } = 5;
    public string[] Queues { get; init; } = ["default", "critical", "low"];
}
