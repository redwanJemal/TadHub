using Identity.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Identity.Contracts;

/// <summary>
/// Service for managing platform admin users.
/// </summary>
public interface IAdminUserService
{
    /// <summary>
    /// Lists all platform admin users with pagination.
    /// </summary>
    Task<PagedList<AdminUserDto>> ListAsync(QueryParameters qp, CancellationToken ct = default);

    /// <summary>
    /// Gets an admin user by ID.
    /// </summary>
    Task<Result<AdminUserDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets an admin user by user profile ID.
    /// </summary>
    Task<Result<AdminUserDto>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Creates a new admin user.
    /// </summary>
    Task<Result<AdminUserDto>> CreateAsync(CreateAdminUserRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates an admin user.
    /// </summary>
    Task<Result<AdminUserDto>> UpdateAsync(Guid id, UpdateAdminUserRequest request, CancellationToken ct = default);

    /// <summary>
    /// Removes admin status from a user (deletes AdminUser record, keeps UserProfile).
    /// </summary>
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Checks if a user is a platform admin.
    /// </summary>
    Task<bool> IsAdminAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Checks if a user is a super admin.
    /// </summary>
    Task<bool> IsSuperAdminAsync(Guid userId, CancellationToken ct = default);
}
