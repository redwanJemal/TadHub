using ReferenceData.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace ReferenceData.Contracts;

/// <summary>
/// Service interface for country payment packages.
/// Tenant-scoped: each Tadbeer center manages its own pricing packages.
/// </summary>
public interface ICountryPackageService
{
    /// <summary>
    /// Lists country packages with filtering and pagination.
    /// </summary>
    Task<PagedList<CountryPackageListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);

    /// <summary>
    /// Gets a country package by ID.
    /// </summary>
    Task<Result<CountryPackageDto>> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets packages for a specific country.
    /// </summary>
    Task<List<CountryPackageListDto>> GetByCountryAsync(Guid tenantId, Guid countryId, CancellationToken ct = default);

    /// <summary>
    /// Gets the default active package for a country.
    /// </summary>
    Task<Result<CountryPackageDto>> GetDefaultByCountryAsync(Guid tenantId, Guid countryId, CancellationToken ct = default);

    /// <summary>
    /// Creates a new country package.
    /// </summary>
    Task<Result<CountryPackageDto>> CreateAsync(Guid tenantId, CreateCountryPackageRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing country package.
    /// </summary>
    Task<Result<CountryPackageDto>> UpdateAsync(Guid tenantId, Guid id, UpdateCountryPackageRequest request, CancellationToken ct = default);

    /// <summary>
    /// Soft-deletes a country package.
    /// </summary>
    Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default);
}
