namespace TadHub.Infrastructure.Settings;

public sealed class MeilisearchSettings
{
    public const string SectionName = "Meilisearch";

    public string Url { get; init; } = "http://localhost:7700";
    public string ApiKey { get; init; } = string.Empty;
    public string IndexPrefix { get; init; } = "saaskit_";
}
