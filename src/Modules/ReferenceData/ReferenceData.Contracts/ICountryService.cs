using ReferenceData.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace ReferenceData.Contracts;

/// <summary>
/// Service interface for country reference data.
/// Countries are global (not tenant-scoped) and cached.
/// </summary>
public interface ICountryService
{
    /// <summary>
    /// Gets all active countries.
    /// Results are cached and ordered by DisplayOrder, then Name.
    /// </summary>
    /// <param name="locale">Optional locale for name resolution (en/ar)</param>
    Task<List<CountryDto>> GetAllAsync(string? locale = null, CancellationToken ct = default);

    /// <summary>
    /// Gets countries with filtering and pagination.
    /// Filters: filter[code], filter[isActive], filter[name][contains]
    /// Sort: sort=nameEn, sort=-displayOrder
    /// </summary>
    Task<PagedList<CountryDto>> ListAsync(QueryParameters qp, CancellationToken ct = default);

    /// <summary>
    /// Gets a country by ID.
    /// </summary>
    Task<Result<CountryDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a country by ISO alpha-2 code.
    /// </summary>
    Task<Result<CountryDto>> GetByCodeAsync(string code, CancellationToken ct = default);

    /// <summary>
    /// Gets lightweight country references for dropdowns.
    /// Only active countries, ordered by display order.
    /// </summary>
    Task<List<CountryRefDto>> GetReferencesAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets common Tadbeer nationalities (prioritized countries).
    /// Philippines, Indonesia, Ethiopia, India, Sri Lanka, Nepal, Bangladesh, Uganda, Kenya.
    /// </summary>
    Task<List<CountryRefDto>> GetCommonNationalitiesAsync(CancellationToken ct = default);
}
