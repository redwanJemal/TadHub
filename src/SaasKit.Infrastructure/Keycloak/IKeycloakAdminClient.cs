using SaasKit.Infrastructure.Keycloak.Models;

namespace SaasKit.Infrastructure.Keycloak;

/// <summary>
/// Client for Keycloak Admin REST API.
/// Provides user management operations for the Identity module.
/// </summary>
public interface IKeycloakAdminClient
{
    /// <summary>
    /// Gets a user by their Keycloak ID.
    /// </summary>
    /// <param name="userId">The Keycloak user ID (UUID).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user representation, or null if not found.</returns>
    Task<KeycloakUserRepresentation?> GetUserAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their email address.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user representation, or null if not found.</returns>
    Task<KeycloakUserRepresentation?> GetUserByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their username.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user representation, or null if not found.</returns>
    Task<KeycloakUserRepresentation?> GetUserByUsernameAsync(
        string username,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new user in Keycloak.
    /// </summary>
    /// <param name="user">The user representation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created user's Keycloak ID.</returns>
    Task<string> CreateUserAsync(
        KeycloakUserRepresentation user,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing user in Keycloak.
    /// </summary>
    /// <param name="userId">The Keycloak user ID.</param>
    /// <param name="user">The updated user representation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task UpdateUserAsync(
        string userId,
        KeycloakUserRepresentation user,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a user from Keycloak.
    /// </summary>
    /// <param name="userId">The Keycloak user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteUserAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email verification email to the user.
    /// </summary>
    /// <param name="userId">The Keycloak user ID.</param>
    /// <param name="redirectUri">Optional redirect URI after verification.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendVerificationEmailAsync(
        string userId,
        string? redirectUri = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a password reset email to the user.
    /// </summary>
    /// <param name="userId">The Keycloak user ID.</param>
    /// <param name="redirectUri">Optional redirect URI after reset.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendPasswordResetEmailAsync(
        string userId,
        string? redirectUri = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets a new password for the user.
    /// </summary>
    /// <param name="userId">The Keycloak user ID.</param>
    /// <param name="password">The new password.</param>
    /// <param name="temporary">If true, user must change password on next login.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ResetPasswordAsync(
        string userId,
        string password,
        bool temporary = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all realm roles available.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of realm roles.</returns>
    Task<IReadOnlyList<KeycloakRoleRepresentation>> GetRealmRolesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the realm roles assigned to a user.
    /// </summary>
    /// <param name="userId">The Keycloak user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of assigned realm roles.</returns>
    Task<IReadOnlyList<KeycloakRoleRepresentation>> GetUserRealmRolesAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns realm roles to a user.
    /// </summary>
    /// <param name="userId">The Keycloak user ID.</param>
    /// <param name="roles">The roles to assign.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task AssignRealmRolesAsync(
        string userId,
        IEnumerable<KeycloakRoleRepresentation> roles,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes realm roles from a user.
    /// </summary>
    /// <param name="userId">The Keycloak user ID.</param>
    /// <param name="roles">The roles to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RemoveRealmRolesAsync(
        string userId,
        IEnumerable<KeycloakRoleRepresentation> roles,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enables or disables a user account.
    /// </summary>
    /// <param name="userId">The Keycloak user ID.</param>
    /// <param name="enabled">Whether the account should be enabled.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SetUserEnabledAsync(
        string userId,
        bool enabled,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users matching search criteria.
    /// </summary>
    /// <param name="search">Search string (matches username, email, first/last name).</param>
    /// <param name="first">First result index (for pagination).</param>
    /// <param name="max">Maximum results to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of matching users.</returns>
    Task<IReadOnlyList<KeycloakUserRepresentation>> SearchUsersAsync(
        string? search = null,
        int first = 0,
        int max = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts users matching search criteria.
    /// </summary>
    /// <param name="search">Optional search string.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Count of matching users.</returns>
    Task<int> CountUsersAsync(
        string? search = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs out all sessions for a user.
    /// </summary>
    /// <param name="userId">The Keycloak user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task LogoutUserAsync(
        string userId,
        CancellationToken cancellationToken = default);
}
