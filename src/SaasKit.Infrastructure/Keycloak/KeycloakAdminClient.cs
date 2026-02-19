using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SaasKit.Infrastructure.Keycloak.Models;
using SaasKit.Infrastructure.Settings;

namespace SaasKit.Infrastructure.Keycloak;

/// <summary>
/// HttpClient-based implementation of the Keycloak Admin REST API client.
/// Authenticates using service account (client credentials grant).
/// </summary>
public sealed class KeycloakAdminClient : IKeycloakAdminClient
{
    private readonly HttpClient _httpClient;
    private readonly KeycloakSettings _settings;
    private readonly ILogger<KeycloakAdminClient> _logger;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    // Buffer to refresh token before it actually expires
    private static readonly TimeSpan TokenExpiryBuffer = TimeSpan.FromMinutes(1);

    public KeycloakAdminClient(
        HttpClient httpClient,
        IOptions<KeycloakSettings> settings,
        ILogger<KeycloakAdminClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    #region User Operations

    public async Task<KeycloakUserRepresentation?> GetUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var response = await _httpClient.GetAsync(
            $"users/{userId}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        await EnsureSuccessAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<KeycloakUserRepresentation>(
            cancellationToken: cancellationToken);
    }

    public async Task<KeycloakUserRepresentation?> GetUserByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var response = await _httpClient.GetAsync(
            $"users?email={Uri.EscapeDataString(email)}&exact=true",
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
        var users = await response.Content.ReadFromJsonAsync<List<KeycloakUserRepresentation>>(
            cancellationToken: cancellationToken);

        return users?.FirstOrDefault();
    }

    public async Task<KeycloakUserRepresentation?> GetUserByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var response = await _httpClient.GetAsync(
            $"users?username={Uri.EscapeDataString(username)}&exact=true",
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);
        var users = await response.Content.ReadFromJsonAsync<List<KeycloakUserRepresentation>>(
            cancellationToken: cancellationToken);

        return users?.FirstOrDefault();
    }

    public async Task<string> CreateUserAsync(
        KeycloakUserRepresentation user,
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var response = await _httpClient.PostAsJsonAsync("users", user, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        // Keycloak returns the user ID in the Location header
        var locationHeader = response.Headers.Location?.ToString();
        if (string.IsNullOrEmpty(locationHeader))
            throw new InvalidOperationException("Keycloak did not return user ID in Location header");

        // Location format: /admin/realms/{realm}/users/{userId}
        var userId = locationHeader.Split('/').Last();
        _logger.LogInformation("Created Keycloak user {UserId} with email {Email}", userId, user.Email);

        return userId;
    }

    public async Task UpdateUserAsync(
        string userId,
        KeycloakUserRepresentation user,
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var response = await _httpClient.PutAsJsonAsync($"users/{userId}", user, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        _logger.LogInformation("Updated Keycloak user {UserId}", userId);
    }

    public async Task DeleteUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var response = await _httpClient.DeleteAsync($"users/{userId}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        _logger.LogInformation("Deleted Keycloak user {UserId}", userId);
    }

    public async Task<IReadOnlyList<KeycloakUserRepresentation>> SearchUsersAsync(
        string? search = null,
        int first = 0,
        int max = 100,
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var query = $"users?first={first}&max={max}";
        if (!string.IsNullOrEmpty(search))
            query += $"&search={Uri.EscapeDataString(search)}";

        var response = await _httpClient.GetAsync(query, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        var users = await response.Content.ReadFromJsonAsync<List<KeycloakUserRepresentation>>(
            cancellationToken: cancellationToken);

        return users ?? [];
    }

    public async Task<int> CountUsersAsync(
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var query = "users/count";
        if (!string.IsNullOrEmpty(search))
            query += $"?search={Uri.EscapeDataString(search)}";

        var response = await _httpClient.GetAsync(query, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        var count = await response.Content.ReadAsStringAsync(cancellationToken);
        return int.Parse(count);
    }

    #endregion

    #region Email Actions

    public async Task SendVerificationEmailAsync(
        string userId,
        string? redirectUri = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var endpoint = $"users/{userId}/send-verify-email";
        if (!string.IsNullOrEmpty(redirectUri))
            endpoint += $"?redirect_uri={Uri.EscapeDataString(redirectUri)}";

        var response = await _httpClient.PutAsync(endpoint, null, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        _logger.LogInformation("Sent verification email to user {UserId}", userId);
    }

    public async Task SendPasswordResetEmailAsync(
        string userId,
        string? redirectUri = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var actions = new[] { "UPDATE_PASSWORD" };
        var endpoint = $"users/{userId}/execute-actions-email";
        if (!string.IsNullOrEmpty(redirectUri))
            endpoint += $"?redirect_uri={Uri.EscapeDataString(redirectUri)}";

        var response = await _httpClient.PutAsJsonAsync(endpoint, actions, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        _logger.LogInformation("Sent password reset email to user {UserId}", userId);
    }

    public async Task ResetPasswordAsync(
        string userId,
        string password,
        bool temporary = false,
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var credential = new KeycloakCredentialRepresentation
        {
            Type = "password",
            Value = password,
            Temporary = temporary
        };

        var response = await _httpClient.PutAsJsonAsync(
            $"users/{userId}/reset-password",
            credential,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        _logger.LogInformation(
            "Reset password for user {UserId} (temporary: {Temporary})",
            userId, temporary);
    }

    #endregion

    #region Role Operations

    public async Task<IReadOnlyList<KeycloakRoleRepresentation>> GetRealmRolesAsync(
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var response = await _httpClient.GetAsync("roles", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        var roles = await response.Content.ReadFromJsonAsync<List<KeycloakRoleRepresentation>>(
            cancellationToken: cancellationToken);

        return roles ?? [];
    }

    public async Task<IReadOnlyList<KeycloakRoleRepresentation>> GetUserRealmRolesAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var response = await _httpClient.GetAsync(
            $"users/{userId}/role-mappings/realm",
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        var roles = await response.Content.ReadFromJsonAsync<List<KeycloakRoleRepresentation>>(
            cancellationToken: cancellationToken);

        return roles ?? [];
    }

    public async Task AssignRealmRolesAsync(
        string userId,
        IEnumerable<KeycloakRoleRepresentation> roles,
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var roleList = roles.ToList();
        var response = await _httpClient.PostAsJsonAsync(
            $"users/{userId}/role-mappings/realm",
            roleList,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        _logger.LogInformation(
            "Assigned {Count} realm roles to user {UserId}",
            roleList.Count, userId);
    }

    public async Task RemoveRealmRolesAsync(
        string userId,
        IEnumerable<KeycloakRoleRepresentation> roles,
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var roleList = roles.ToList();
        var request = new HttpRequestMessage(HttpMethod.Delete, $"users/{userId}/role-mappings/realm")
        {
            Content = JsonContent.Create(roleList)
        };

        var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        _logger.LogInformation(
            "Removed {Count} realm roles from user {UserId}",
            roleList.Count, userId);
    }

    #endregion

    #region Account Operations

    public async Task SetUserEnabledAsync(
        string userId,
        bool enabled,
        CancellationToken cancellationToken = default)
    {
        var user = await GetUserAsync(userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User {userId} not found");

        user.Enabled = enabled;
        await UpdateUserAsync(userId, user, cancellationToken);

        _logger.LogInformation(
            "Set user {UserId} enabled status to {Enabled}",
            userId, enabled);
    }

    public async Task LogoutUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync(cancellationToken);

        var response = await _httpClient.PostAsync(
            $"users/{userId}/logout",
            null,
            cancellationToken);

        await EnsureSuccessAsync(response, cancellationToken);

        _logger.LogInformation("Logged out all sessions for user {UserId}", userId);
    }

    #endregion

    #region Authentication

    private async Task EnsureAuthenticatedAsync(CancellationToken cancellationToken)
    {
        // Quick check without lock
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _accessToken);
            return;
        }

        await _tokenLock.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring lock
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _accessToken);
                return;
            }

            await RefreshTokenAsync(cancellationToken);
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    private async Task RefreshTokenAsync(CancellationToken cancellationToken)
    {
        // Parse base URL from Authority (e.g., http://localhost:8080/realms/saas-platform)
        var authorityUri = new Uri(_settings.Authority);
        var baseUrl = $"{authorityUri.Scheme}://{authorityUri.Host}:{authorityUri.Port}";

        // Token endpoint for client credentials
        var tokenEndpoint = $"{_settings.Authority}/protocol/openid-connect/token";

        var tokenRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _settings.ClientId,
            ["client_secret"] = _settings.ClientSecret
        });

        using var tempClient = new HttpClient();
        var response = await tempClient.PostAsync(tokenEndpoint, tokenRequest, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError(
                "Failed to obtain Keycloak access token: {StatusCode} {Error}",
                response.StatusCode, errorContent);
            throw new InvalidOperationException($"Failed to authenticate with Keycloak: {errorContent}");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(
            cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Failed to parse Keycloak token response");

        _accessToken = tokenResponse.AccessToken;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn) - TokenExpiryBuffer;

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _accessToken);

        _logger.LogDebug(
            "Obtained Keycloak access token, expires at {Expiry}",
            _tokenExpiry);
    }

    #endregion

    #region Error Handling

    private async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        KeycloakErrorResponse? error = null;

        try
        {
            error = JsonSerializer.Deserialize<KeycloakErrorResponse>(content);
        }
        catch
        {
            // Ignore deserialization errors
        }

        var errorMessage = error?.ErrorDescription
            ?? error?.ErrorMessage
            ?? error?.Error
            ?? content;

        _logger.LogError(
            "Keycloak API error: {StatusCode} {Error}",
            response.StatusCode, errorMessage);

        throw response.StatusCode switch
        {
            HttpStatusCode.NotFound => new KeyNotFoundException(errorMessage),
            HttpStatusCode.Conflict => new InvalidOperationException($"Conflict: {errorMessage}"),
            HttpStatusCode.BadRequest => new ArgumentException(errorMessage),
            HttpStatusCode.Forbidden => new UnauthorizedAccessException(errorMessage),
            HttpStatusCode.Unauthorized => new UnauthorizedAccessException(errorMessage),
            _ => new HttpRequestException($"Keycloak API error ({response.StatusCode}): {errorMessage}")
        };
    }

    #endregion
}
