using Authorization.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Authorization.Contracts;

/// <summary>
/// Service for managing roles, permissions, and user authorization.
/// </summary>
public interface IAuthorizationModuleService
{
    #region Permission Operations

    /// <summary>
    /// Gets all permissions with optional filtering.
    /// </summary>
    Task<PagedList<PermissionDto>> GetPermissionsAsync(QueryParameters qp, CancellationToken ct = default);

    /// <summary>
    /// Gets a permission by ID.
    /// </summary>
    Task<Result<PermissionDto>> GetPermissionByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a permission by name.
    /// </summary>
    Task<Result<PermissionDto>> GetPermissionByNameAsync(string name, CancellationToken ct = default);

    #endregion

    #region Role Operations

    /// <summary>
    /// Gets roles for a tenant with optional filtering.
    /// </summary>
    Task<PagedList<RoleDto>> GetRolesAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);

    /// <summary>
    /// Gets a role by ID.
    /// </summary>
    Task<Result<RoleDto>> GetRoleByIdAsync(Guid tenantId, Guid roleId, CancellationToken ct = default);

    /// <summary>
    /// Creates a new role in a tenant.
    /// </summary>
    Task<Result<RoleDto>> CreateRoleAsync(Guid tenantId, CreateRoleRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates a role.
    /// </summary>
    Task<Result<RoleDto>> UpdateRoleAsync(Guid tenantId, Guid roleId, UpdateRoleRequest request, CancellationToken ct = default);

    /// <summary>
    /// Deletes a role.
    /// </summary>
    Task<Result<bool>> DeleteRoleAsync(Guid tenantId, Guid roleId, CancellationToken ct = default);

    /// <summary>
    /// Seeds default roles for a new tenant.
    /// </summary>
    Task SeedDefaultRolesAsync(Guid tenantId, Guid ownerUserId, CancellationToken ct = default);

    #endregion

    #region User Role Operations

    /// <summary>
    /// Gets roles assigned to a user in a tenant.
    /// </summary>
    Task<IReadOnlyList<RoleDto>> GetUserRolesAsync(Guid tenantId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    Task<Result<UserRoleDto>> AssignRoleAsync(Guid tenantId, AssignRoleRequest request, CancellationToken ct = default);

    /// <summary>
    /// Removes a role from a user.
    /// </summary>
    Task<Result<bool>> RemoveRoleAsync(Guid tenantId, Guid userId, Guid roleId, CancellationToken ct = default);

    #endregion

    #region Authorization Checks

    /// <summary>
    /// Gets all permissions for a user in a tenant.
    /// Results are cached in Redis for 5 minutes.
    /// </summary>
    Task<UserPermissionsDto> GetUserPermissionsAsync(Guid tenantId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Checks if a user has a specific permission.
    /// </summary>
    Task<bool> HasPermissionAsync(Guid tenantId, Guid userId, string permission, CancellationToken ct = default);

    /// <summary>
    /// Checks if a user has any of the specified permissions.
    /// </summary>
    Task<bool> HasAnyPermissionAsync(Guid tenantId, Guid userId, params string[] permissions);

    /// <summary>
    /// Checks if a user has all of the specified permissions.
    /// </summary>
    Task<bool> HasAllPermissionsAsync(Guid tenantId, Guid userId, params string[] permissions);

    /// <summary>
    /// Invalidates the cached permissions for a user.
    /// Call this when roles/permissions change.
    /// </summary>
    Task InvalidateUserPermissionsCacheAsync(Guid tenantId, Guid userId, CancellationToken ct = default);

    #endregion
}
