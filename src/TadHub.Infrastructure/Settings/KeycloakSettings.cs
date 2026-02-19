namespace TadHub.Infrastructure.Settings;

public sealed class KeycloakSettings
{
    public const string SectionName = "Keycloak";

    public string Authority { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public bool RequireHttpsMetadata { get; init; } = true;
    public bool ValidateIssuer { get; init; } = true;
    public bool ValidateAudience { get; init; } = true;
}
