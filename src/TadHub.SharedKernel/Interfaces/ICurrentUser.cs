namespace TadHub.SharedKernel.Interfaces;

/// <summary>
/// Provides access to the current authenticated user.
/// Populated from JWT claims by the authentication middleware.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// The unique identifier of the current user.
    /// </summary>
    Guid UserId { get; }

    /// <summary>
    /// The email address of the current user.
    /// </summary>
    string Email { get; }

    /// <summary>
    /// Indicates whether the current request is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// The roles assigned to the current user.
    /// </summary>
    IReadOnlyList<string> Roles { get; }

    /// <summary>
    /// Checks if the current user has a specific role.
    /// </summary>
    bool HasRole(string role);

    /// <summary>
    /// Checks if the current user has any of the specified roles.
    /// </summary>
    bool HasAnyRole(params string[] roles);

    /// <summary>
    /// The Keycloak user ID (sub claim) as a Guid.
    /// This is the external identity provider ID, not the internal user_profiles.Id.
    /// </summary>
    Guid KeycloakUserId { get; }
}
