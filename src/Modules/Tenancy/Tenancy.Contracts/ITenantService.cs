using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;
using Tenancy.Contracts.DTOs;

namespace Tenancy.Contracts;

/// <summary>
/// Service for managing tenants and memberships.
/// </summary>
public interface ITenantService
{
    #region Tenant Operations

    /// <summary>
    /// Gets a tenant by ID.
    /// </summary>
    Task<Result<TenantDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a tenant by slug.
    /// </summary>
    Task<Result<TenantDto>> GetBySlugAsync(string slug, CancellationToken ct = default);

    /// <summary>
    /// Lists tenants the user is a member of.
    /// </summary>
    Task<PagedList<TenantDto>> ListUserTenantsAsync(Guid userId, QueryParameters qp, CancellationToken ct = default);

    /// <summary>
    /// Lists all tenants (for platform admins).
    /// </summary>
    Task<PagedList<TenantDto>> ListAllTenantsAsync(QueryParameters qp, CancellationToken ct = default);

    /// <summary>
    /// Creates a new tenant. The current user becomes the owner.
    /// </summary>
    Task<Result<TenantDto>> CreateAsync(CreateTenantRequest request, CancellationToken ct = default);

    /// <summary>
    /// Creates a new tenant with a dedicated owner user (admin-only).
    /// Creates the user in Keycloak, provisions a local profile, and sets up ownership.
    /// </summary>
    Task<Result<TenantDto>> AdminCreateAsync(AdminCreateTenantRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates a tenant.
    /// </summary>
    Task<Result<TenantDto>> UpdateAsync(Guid id, UpdateTenantRequest request, CancellationToken ct = default);

    /// <summary>
    /// Suspends a tenant.
    /// </summary>
    Task<Result<bool>> SuspendAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Reactivates a suspended tenant.
    /// </summary>
    Task<Result<bool>> ReactivateAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Soft-deletes a tenant.
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid id, CancellationToken ct = default);

    #endregion

    #region Member Operations

    /// <summary>
    /// Gets members of a tenant with filtering and pagination.
    /// </summary>
    Task<PagedList<TenantMemberDto>> GetMembersAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);

    /// <summary>
    /// Gets a specific member.
    /// </summary>
    Task<Result<TenantMemberDto>> GetMemberAsync(Guid tenantId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Adds a user as a member of a tenant.
    /// </summary>
    Task<Result<TenantMemberDto>> AddMemberAsync(Guid tenantId, Guid userId, bool isOwner = false, CancellationToken ct = default);

    /// <summary>
    /// Removes a member from a tenant.
    /// </summary>
    Task<Result<bool>> RemoveMemberAsync(Guid tenantId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Checks if a user is a member of a tenant.
    /// </summary>
    Task<bool> IsMemberAsync(Guid tenantId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Checks if a user is an owner of a tenant.
    /// </summary>
    Task<bool> IsOwnerAsync(Guid tenantId, Guid userId, CancellationToken ct = default);

    #endregion

    #region Invitation Operations

    /// <summary>
    /// Creates an invitation to join a tenant.
    /// </summary>
    Task<Result<TenantInvitationDto>> InviteMemberAsync(Guid tenantId, InviteMemberRequest request, CancellationToken ct = default);

    /// <summary>
    /// Gets pending invitations for a tenant.
    /// </summary>
    Task<PagedList<TenantInvitationDto>> GetInvitationsAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);

    /// <summary>
    /// Accepts an invitation using the token.
    /// </summary>
    Task<Result<TenantMemberDto>> AcceptInvitationAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// Revokes a pending invitation.
    /// </summary>
    Task<Result<bool>> RevokeInvitationAsync(Guid tenantId, Guid invitationId, CancellationToken ct = default);

    /// <summary>
    /// Gets an invitation by token (for the accept invitation page).
    /// </summary>
    Task<Result<TenantInvitationDto>> GetInvitationByTokenAsync(string token, CancellationToken ct = default);

    #endregion

    #region Settings Operations

    /// <summary>
    /// Gets the raw settings JSON for a tenant.
    /// </summary>
    Task<Result<string?>> GetSettingsJsonAsync(Guid tenantId, CancellationToken ct = default);

    /// <summary>
    /// Updates a specific section within the tenant settings JSON.
    /// </summary>
    Task<Result<bool>> UpdateSettingsSectionAsync(Guid tenantId, string sectionKey, string sectionJson, CancellationToken ct = default);

    #endregion
}
