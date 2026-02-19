using Identity.Contracts.DTOs;
using SaasKit.SharedKernel.Api;
using SaasKit.SharedKernel.Models;

namespace Identity.Contracts;

/// <summary>
/// Service for managing user profiles.
/// Users are synced from Keycloak and enriched with application-specific data.
/// </summary>
public interface IIdentityService
{
    /// <summary>
    /// Gets a user profile by ID.
    /// </summary>
    /// <param name="id">The user profile ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The user profile if found.</returns>
    Task<Result<UserProfileDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a user profile by Keycloak ID (sub claim).
    /// </summary>
    /// <param name="keycloakId">The Keycloak user ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The user profile if found.</returns>
    Task<Result<UserProfileDto>> GetByKeycloakIdAsync(string keycloakId, CancellationToken ct = default);

    /// <summary>
    /// Gets a user profile by email address.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The user profile if found.</returns>
    Task<Result<UserProfileDto>> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>
    /// Creates a new user profile.
    /// </summary>
    /// <param name="request">The creation request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created user profile.</returns>
    Task<Result<UserProfileDto>> CreateAsync(CreateUserProfileRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing user profile.
    /// </summary>
    /// <param name="id">The user profile ID.</param>
    /// <param name="request">The update request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated user profile.</returns>
    Task<Result<UserProfileDto>> UpdateAsync(Guid id, UpdateUserProfileRequest request, CancellationToken ct = default);

    /// <summary>
    /// Deactivates a user profile.
    /// </summary>
    /// <param name="id">The user profile ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if deactivated successfully.</returns>
    Task<Result<bool>> DeactivateAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Reactivates a user profile.
    /// </summary>
    /// <param name="id">The user profile ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if reactivated successfully.</returns>
    Task<Result<bool>> ReactivateAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Lists user profiles with filtering, sorting, and pagination.
    /// </summary>
    /// <param name="qp">Query parameters (filters, sort, pagination).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paged list of user profiles.</returns>
    Task<PagedList<UserProfileDto>> ListAsync(QueryParameters qp, CancellationToken ct = default);

    /// <summary>
    /// Records a user's last login time.
    /// </summary>
    /// <param name="id">The user profile ID.</param>
    /// <param name="ct">Cancellation token.</param>
    Task RecordLoginAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets or creates a user profile from Keycloak data.
    /// Used for JIT (just-in-time) provisioning.
    /// </summary>
    /// <param name="keycloakId">The Keycloak user ID.</param>
    /// <param name="email">The user's email.</param>
    /// <param name="firstName">The user's first name.</param>
    /// <param name="lastName">The user's last name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The existing or newly created user profile.</returns>
    Task<Result<UserProfileDto>> GetOrCreateFromKeycloakAsync(
        string keycloakId,
        string email,
        string firstName,
        string lastName,
        CancellationToken ct = default);
}
