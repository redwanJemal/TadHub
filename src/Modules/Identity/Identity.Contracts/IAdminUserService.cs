using Identity.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Identity.Contracts;

/// <summary>
/// Service for managing platform staff members.
/// </summary>
public interface IPlatformStaffService
{
    /// <summary>
    /// Lists all platform staff with pagination.
    /// </summary>
    Task<PagedList<PlatformStaffDto>> ListAsync(QueryParameters qp, CancellationToken ct = default);

    /// <summary>
    /// Gets a platform staff member by ID.
    /// </summary>
    Task<Result<PlatformStaffDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a platform staff member by user profile ID.
    /// </summary>
    Task<Result<PlatformStaffDto>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Creates a new platform staff member.
    /// </summary>
    Task<Result<PlatformStaffDto>> CreateAsync(CreatePlatformStaffRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates a platform staff member.
    /// </summary>
    Task<Result<PlatformStaffDto>> UpdateAsync(Guid id, UpdatePlatformStaffRequest request, CancellationToken ct = default);

    /// <summary>
    /// Removes platform staff status from a user (deletes PlatformStaff record, keeps UserProfile).
    /// </summary>
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Checks if a user is a platform staff member.
    /// </summary>
    Task<bool> IsStaffAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Checks if a user has a specific platform role (e.g., "super-admin", "admin", "finance").
    /// </summary>
    Task<bool> HasRoleAsync(Guid userId, string role, CancellationToken ct = default);
}
