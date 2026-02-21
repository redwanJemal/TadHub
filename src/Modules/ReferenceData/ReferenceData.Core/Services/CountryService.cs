using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using ReferenceData.Contracts;
using ReferenceData.Contracts.DTOs;
using ReferenceData.Core.Entities;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace ReferenceData.Core.Services;

/// <summary>
/// Service implementation for country reference data.
/// Uses Redis caching for frequently accessed data.
/// </summary>
public class CountryService : ICountryService
{
    private readonly AppDbContext _db;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CountryService> _logger;

    private const string CacheKeyAll = "ref:countries:all";
    private const string CacheKeyRefs = "ref:countries:refs";
    private const string CacheKeyCommon = "ref:countries:common";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    // Filterable fields
    private static readonly Dictionary<string, Expression<Func<Country, object>>> Filters = new()
    {
        ["code"] = x => x.Code,
        ["alpha3Code"] = x => x.Alpha3Code,
        ["isActive"] = x => x.IsActive,
        ["isCommonNationality"] = x => x.IsCommonNationality
    };

    // Sortable fields
    private static readonly Dictionary<string, Expression<Func<Country, object>>> Sortable = new()
    {
        ["code"] = x => x.Code,
        ["displayOrder"] = x => x.DisplayOrder,
        ["createdAt"] = x => x.CreatedAt
    };

    public CountryService(AppDbContext db, IDistributedCache cache, ILogger<CountryService> logger)
    {
        _db = db;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<CountryDto>> GetAllAsync(string? locale = null, CancellationToken ct = default)
    {
        var countries = await _db.Set<Country>()
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name.En)
            .ToListAsync(ct);

        return countries.Select(MapToDto).ToList();
    }

    public async Task<PagedList<CountryDto>> ListAsync(QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<Country>()
            .AsNoTracking()
            .ApplyFilters(qp.Filters, Filters)
            .ApplySort(qp.GetSortFields(), Sortable);

        return await query
            .Select(x => MapToDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<CountryDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var country = await _db.Set<Country>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (country is null)
            return Result<CountryDto>.NotFound("Country not found");

        return Result<CountryDto>.Success(MapToDto(country));
    }

    public async Task<Result<CountryDto>> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var country = await _db.Set<Country>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code.ToUpperInvariant(), ct);

        if (country is null)
            return Result<CountryDto>.NotFound($"Country with code '{code}' not found");

        return Result<CountryDto>.Success(MapToDto(country));
    }

    public async Task<List<CountryRefDto>> GetReferencesAsync(CancellationToken ct = default)
    {
        var countries = await _db.Set<Country>()
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name.En)
            .Select(x => new CountryRefDto
            {
                Id = x.Id,
                Code = x.Code,
                NameEn = x.Name.En,
                NameAr = x.Name.Ar
            })
            .ToListAsync(ct);

        return countries;
    }

    public async Task<List<CountryRefDto>> GetCommonNationalitiesAsync(CancellationToken ct = default)
    {
        var countries = await _db.Set<Country>()
            .AsNoTracking()
            .Where(x => x.IsActive && x.IsCommonNationality)
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new CountryRefDto
            {
                Id = x.Id,
                Code = x.Code,
                NameEn = x.Name.En,
                NameAr = x.Name.Ar
            })
            .ToListAsync(ct);

        return countries;
    }

    private static CountryDto MapToDto(Country c) => new()
    {
        Id = c.Id,
        Code = c.Code,
        Alpha3Code = c.Alpha3Code,
        NameEn = c.Name.En,
        NameAr = c.Name.Ar,
        NationalityEn = c.Nationality.En,
        NationalityAr = c.Nationality.Ar,
        DialingCode = c.DialingCode,
        IsActive = c.IsActive,
        DisplayOrder = c.DisplayOrder
    };
}
