using System.Text.Json.Serialization;

namespace TadHub.Infrastructure.Keycloak.Models;

/// <summary>
/// Keycloak user representation for Admin API.
/// </summary>
public sealed class KeycloakUserRepresentation
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("emailVerified")]
    public bool EmailVerified { get; set; }

    [JsonPropertyName("createdTimestamp")]
    public long? CreatedTimestamp { get; set; }

    [JsonPropertyName("attributes")]
    public Dictionary<string, List<string>>? Attributes { get; set; }

    [JsonPropertyName("credentials")]
    public List<KeycloakCredentialRepresentation>? Credentials { get; set; }

    [JsonPropertyName("requiredActions")]
    public List<string>? RequiredActions { get; set; }

    [JsonPropertyName("realmRoles")]
    public List<string>? RealmRoles { get; set; }

    [JsonPropertyName("groups")]
    public List<string>? Groups { get; set; }
}

/// <summary>
/// Keycloak credential representation.
/// </summary>
public sealed class KeycloakCredentialRepresentation
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "password";

    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("temporary")]
    public bool Temporary { get; set; }
}

/// <summary>
/// Keycloak role representation.
/// </summary>
public sealed class KeycloakRoleRepresentation
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("composite")]
    public bool Composite { get; set; }

    [JsonPropertyName("clientRole")]
    public bool ClientRole { get; set; }

    [JsonPropertyName("containerId")]
    public string? ContainerId { get; set; }
}

/// <summary>
/// Keycloak token response for client credentials grant.
/// </summary>
public sealed class KeycloakTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_expires_in")]
    public int RefreshExpiresIn { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("not-before-policy")]
    public int NotBeforePolicy { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;
}

/// <summary>
/// Keycloak error response.
/// </summary>
public sealed class KeycloakErrorResponse
{
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; set; }

    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; set; }
}
