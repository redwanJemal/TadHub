using ReferenceData.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace ReferenceData.Contracts;

/// <summary>
/// Service interface for MoHRE job categories.
/// Job categories are global (not tenant-scoped) and cached.
/// </summary>
public interface IJobCategoryService
{
    /// <summary>
    /// Gets all active job categories.
    /// Results are cached and ordered by DisplayOrder.
    /// </summary>
    Task<List<JobCategoryDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets job categories with filtering and pagination.
    /// Filters: filter[moHRECode], filter[isActive], filter[name][contains]
    /// Sort: sort=nameEn, sort=-displayOrder
    /// </summary>
    Task<PagedList<JobCategoryDto>> ListAsync(QueryParameters qp, CancellationToken ct = default);

    /// <summary>
    /// Gets a job category by ID.
    /// </summary>
    Task<Result<JobCategoryDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a job category by MoHRE code.
    /// </summary>
    Task<Result<JobCategoryDto>> GetByCodeAsync(string moHRECode, CancellationToken ct = default);

    /// <summary>
    /// Gets lightweight job category references for dropdowns.
    /// Only active categories, ordered by display order.
    /// </summary>
    Task<List<JobCategoryRefDto>> GetReferencesAsync(CancellationToken ct = default);
}
