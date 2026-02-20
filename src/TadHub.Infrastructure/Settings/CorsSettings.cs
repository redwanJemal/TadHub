namespace TadHub.Infrastructure.Settings;

public sealed class CorsSettings
{
    public const string SectionName = "Cors";

    /// <summary>
    /// Comma-separated list of allowed origins (for environment variable support).
    /// Takes precedence over AllowedOrigins array if set.
    /// </summary>
    public string? AllowedOriginsString { get; init; }

    /// <summary>
    /// Array of allowed origins (from JSON config).
    /// </summary>
    public string[] AllowedOrigins { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets the effective allowed origins, preferring the comma-separated string if set.
    /// </summary>
    public string[] GetEffectiveOrigins()
    {
        if (!string.IsNullOrWhiteSpace(AllowedOriginsString))
        {
            return AllowedOriginsString
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
        return AllowedOrigins;
    }
}
