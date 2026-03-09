using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
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
/// Service implementation for country payment packages.
/// </summary>
public class CountryPackageService : ICountryPackageService
{
    private readonly AppDbContext _db;
    private readonly ILogger<CountryPackageService> _logger;

    private static readonly Dictionary<string, Expression<Func<CountryPackage, object>>> FilterableFields = new()
    {
        ["countryId"] = x => x.CountryId,
        ["isActive"] = x => x.IsActive,
        ["isDefault"] = x => x.IsDefault,
        ["currency"] = x => x.Currency,
    };

    private static readonly Dictionary<string, Expression<Func<CountryPackage, object>>> SortableFields = new()
    {
        ["name"] = x => x.Name,
        ["totalPackagePrice"] = x => x.TotalPackagePrice,
        ["effectiveFrom"] = x => x.EffectiveFrom,
        ["createdAt"] = x => x.CreatedAt,
    };

    public CountryPackageService(AppDbContext db, ILogger<CountryPackageService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedList<CountryPackageListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<CountryPackage>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted);

        // Search
        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var search = qp.Search.ToLower();
            query = query.Where(x => x.Name.ToLower().Contains(search));
        }

        query = query
            .ApplyFilters(qp.Filters, FilterableFields)
            .ApplySort(qp.GetSortFields(), SortableFields);

        // Join with countries for name enrichment
        var projected = from pkg in query
                        join c in _db.Set<Country>() on pkg.CountryId equals c.Id into cj
                        from country in cj.DefaultIfEmpty()
                        select new CountryPackageListDto
                        {
                            Id = pkg.Id,
                            CountryId = pkg.CountryId,
                            Name = pkg.Name,
                            IsDefault = pkg.IsDefault,
                            CountryNameEn = country != null ? country.Name.En : null,
                            CountryNameAr = country != null ? country.Name.Ar : null,
                            CountryCode = country != null ? country.Code : null,
                            TotalPackagePrice = pkg.TotalPackagePrice,
                            Currency = pkg.Currency,
                            EffectiveFrom = pkg.EffectiveFrom.ToString("yyyy-MM-dd"),
                            EffectiveTo = pkg.EffectiveTo != null ? pkg.EffectiveTo.Value.ToString("yyyy-MM-dd") : null,
                            IsActive = pkg.IsActive,
                            DefaultGuaranteePeriod = pkg.DefaultGuaranteePeriod.ToString(),
                            CreatedAt = pkg.CreatedAt,
                        };

        return await projected.ToPagedListAsync(qp, ct);
    }

    public async Task<Result<CountryPackageDto>> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var pkg = await _db.Set<CountryPackage>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (pkg is null)
            return Result<CountryPackageDto>.NotFound("Country package not found");

        var dto = await MapToDtoAsync(pkg, ct);
        return Result<CountryPackageDto>.Success(dto);
    }

    public async Task<List<CountryPackageListDto>> GetByCountryAsync(Guid tenantId, Guid countryId, CancellationToken ct = default)
    {
        var query = from pkg in _db.Set<CountryPackage>()
                        .IgnoreQueryFilters()
                        .AsNoTracking()
                        .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.CountryId == countryId)
                    join c in _db.Set<Country>() on pkg.CountryId equals c.Id into cj
                    from country in cj.DefaultIfEmpty()
                    orderby pkg.IsDefault descending, pkg.Name
                    select new CountryPackageListDto
                    {
                        Id = pkg.Id,
                        CountryId = pkg.CountryId,
                        Name = pkg.Name,
                        IsDefault = pkg.IsDefault,
                        CountryNameEn = country != null ? country.Name.En : null,
                        CountryNameAr = country != null ? country.Name.Ar : null,
                        CountryCode = country != null ? country.Code : null,
                        TotalPackagePrice = pkg.TotalPackagePrice,
                        Currency = pkg.Currency,
                        EffectiveFrom = pkg.EffectiveFrom.ToString("yyyy-MM-dd"),
                        EffectiveTo = pkg.EffectiveTo != null ? pkg.EffectiveTo.Value.ToString("yyyy-MM-dd") : null,
                        IsActive = pkg.IsActive,
                        DefaultGuaranteePeriod = pkg.DefaultGuaranteePeriod.ToString(),
                        CreatedAt = pkg.CreatedAt,
                    };

        return await query.ToListAsync(ct);
    }

    public async Task<Result<CountryPackageDto>> GetDefaultByCountryAsync(Guid tenantId, Guid countryId, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var pkg = await _db.Set<CountryPackage>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted
                && x.CountryId == countryId
                && x.IsDefault
                && x.IsActive
                && x.EffectiveFrom <= today
                && (x.EffectiveTo == null || x.EffectiveTo >= today))
            .FirstOrDefaultAsync(ct);

        if (pkg is null)
            return Result<CountryPackageDto>.NotFound($"No default active package found for country {countryId}");

        var dto = await MapToDtoAsync(pkg, ct);
        return Result<CountryPackageDto>.Success(dto);
    }

    public async Task<Result<CountryPackageDto>> CreateAsync(Guid tenantId, CreateCountryPackageRequest request, CancellationToken ct = default)
    {
        // Validate country exists
        var countryExists = await _db.Set<Country>()
            .AsNoTracking()
            .AnyAsync(x => x.Id == request.CountryId, ct);

        if (!countryExists)
            return Result<CountryPackageDto>.ValidationError("Country not found");

        // Parse enums
        if (!Enum.TryParse<SupplierCommissionType>(request.SupplierCommissionType, out var commType))
            return Result<CountryPackageDto>.ValidationError($"Invalid supplier commission type: {request.SupplierCommissionType}");

        if (!Enum.TryParse<DefaultGuaranteePeriod>(request.DefaultGuaranteePeriod, out var guaranteePeriod))
            return Result<CountryPackageDto>.ValidationError($"Invalid guarantee period: {request.DefaultGuaranteePeriod}");

        if (!DateOnly.TryParse(request.EffectiveFrom, out var effectiveFrom))
            return Result<CountryPackageDto>.ValidationError("Invalid effective from date");

        DateOnly? effectiveTo = null;
        if (!string.IsNullOrWhiteSpace(request.EffectiveTo))
        {
            if (!DateOnly.TryParse(request.EffectiveTo, out var parsedTo))
                return Result<CountryPackageDto>.ValidationError("Invalid effective to date");
            effectiveTo = parsedTo;
        }

        // If setting as default, unset other defaults for this country
        if (request.IsDefault)
        {
            await UnsetDefaultForCountryAsync(tenantId, request.CountryId, null, ct);
        }

        var entity = new CountryPackage
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            CountryId = request.CountryId,
            Name = request.Name,
            IsDefault = request.IsDefault,
            MaidCost = request.MaidCost,
            MonthlyAccommodationCost = request.MonthlyAccommodationCost,
            VisaCost = request.VisaCost,
            EmploymentVisaCost = request.EmploymentVisaCost,
            ResidenceVisaCost = request.ResidenceVisaCost,
            MedicalCost = request.MedicalCost,
            TransportationCost = request.TransportationCost,
            TicketCost = request.TicketCost,
            InsuranceCost = request.InsuranceCost,
            EmiratesIdCost = request.EmiratesIdCost,
            OtherCosts = request.OtherCosts,
            TotalPackagePrice = request.TotalPackagePrice,
            SupplierCommission = request.SupplierCommission,
            SupplierCommissionType = commType,
            DefaultGuaranteePeriod = guaranteePeriod,
            Currency = request.Currency,
            EffectiveFrom = effectiveFrom,
            EffectiveTo = effectiveTo,
            IsActive = request.IsActive,
            Notes = request.Notes,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };

        _db.Set<CountryPackage>().Add(entity);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created country package {PackageId} for country {CountryId} in tenant {TenantId}",
            entity.Id, entity.CountryId, tenantId);

        var dto = await MapToDtoAsync(entity, ct);
        return Result<CountryPackageDto>.Success(dto);
    }

    public async Task<Result<CountryPackageDto>> UpdateAsync(Guid tenantId, Guid id, UpdateCountryPackageRequest request, CancellationToken ct = default)
    {
        var entity = await _db.Set<CountryPackage>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (entity is null)
            return Result<CountryPackageDto>.NotFound("Country package not found");

        // Validate country if changing
        if (request.CountryId.HasValue)
        {
            var countryExists = await _db.Set<Country>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.CountryId.Value, ct);
            if (!countryExists)
                return Result<CountryPackageDto>.ValidationError("Country not found");
            entity.CountryId = request.CountryId.Value;
        }

        if (request.Name is not null) entity.Name = request.Name;

        // Handle default toggle
        if (request.IsDefault.HasValue)
        {
            if (request.IsDefault.Value && !entity.IsDefault)
            {
                await UnsetDefaultForCountryAsync(tenantId, entity.CountryId, entity.Id, ct);
            }
            entity.IsDefault = request.IsDefault.Value;
        }

        if (request.MaidCost.HasValue) entity.MaidCost = request.MaidCost.Value;
        if (request.MonthlyAccommodationCost.HasValue) entity.MonthlyAccommodationCost = request.MonthlyAccommodationCost.Value;
        if (request.VisaCost.HasValue) entity.VisaCost = request.VisaCost.Value;
        if (request.EmploymentVisaCost.HasValue) entity.EmploymentVisaCost = request.EmploymentVisaCost.Value;
        if (request.ResidenceVisaCost.HasValue) entity.ResidenceVisaCost = request.ResidenceVisaCost.Value;
        if (request.MedicalCost.HasValue) entity.MedicalCost = request.MedicalCost.Value;
        if (request.TransportationCost.HasValue) entity.TransportationCost = request.TransportationCost.Value;
        if (request.TicketCost.HasValue) entity.TicketCost = request.TicketCost.Value;
        if (request.InsuranceCost.HasValue) entity.InsuranceCost = request.InsuranceCost.Value;
        if (request.EmiratesIdCost.HasValue) entity.EmiratesIdCost = request.EmiratesIdCost.Value;
        if (request.OtherCosts.HasValue) entity.OtherCosts = request.OtherCosts.Value;
        if (request.TotalPackagePrice.HasValue) entity.TotalPackagePrice = request.TotalPackagePrice.Value;
        if (request.SupplierCommission.HasValue) entity.SupplierCommission = request.SupplierCommission.Value;

        if (request.SupplierCommissionType is not null)
        {
            if (!Enum.TryParse<SupplierCommissionType>(request.SupplierCommissionType, out var commType))
                return Result<CountryPackageDto>.ValidationError($"Invalid supplier commission type: {request.SupplierCommissionType}");
            entity.SupplierCommissionType = commType;
        }

        if (request.DefaultGuaranteePeriod is not null)
        {
            if (!Enum.TryParse<DefaultGuaranteePeriod>(request.DefaultGuaranteePeriod, out var gp))
                return Result<CountryPackageDto>.ValidationError($"Invalid guarantee period: {request.DefaultGuaranteePeriod}");
            entity.DefaultGuaranteePeriod = gp;
        }

        if (request.Currency is not null) entity.Currency = request.Currency;

        if (request.EffectiveFrom is not null)
        {
            if (!DateOnly.TryParse(request.EffectiveFrom, out var ef))
                return Result<CountryPackageDto>.ValidationError("Invalid effective from date");
            entity.EffectiveFrom = ef;
        }

        if (request.EffectiveTo is not null)
        {
            if (!DateOnly.TryParse(request.EffectiveTo, out var et))
                return Result<CountryPackageDto>.ValidationError("Invalid effective to date");
            entity.EffectiveTo = et;
        }

        if (request.IsActive.HasValue) entity.IsActive = request.IsActive.Value;
        if (request.Notes is not null) entity.Notes = request.Notes;

        entity.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);

        var dto = await MapToDtoAsync(entity, ct);
        return Result<CountryPackageDto>.Success(dto);
    }

    public async Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Set<CountryPackage>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (entity is null)
            return Result.NotFound("Country package not found");

        entity.MarkAsDeleted();
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted country package {PackageId} in tenant {TenantId}", id, tenantId);
        return Result.Success();
    }

    // ── Private helpers ──

    private async Task UnsetDefaultForCountryAsync(Guid tenantId, Guid countryId, Guid? excludeId, CancellationToken ct)
    {
        var defaults = await _db.Set<CountryPackage>()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId
                && !x.IsDeleted
                && x.CountryId == countryId
                && x.IsDefault
                && (excludeId == null || x.Id != excludeId))
            .ToListAsync(ct);

        foreach (var d in defaults)
        {
            d.IsDefault = false;
            d.UpdatedAt = DateTimeOffset.UtcNow;
        }
    }

    private async Task<CountryPackageDto> MapToDtoAsync(CountryPackage pkg, CancellationToken ct)
    {
        // Fetch country info
        string? countryNameEn = null, countryNameAr = null, countryCode = null;
        var country = await _db.Set<Country>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == pkg.CountryId, ct);

        if (country is not null)
        {
            countryNameEn = country.Name.En;
            countryNameAr = country.Name.Ar;
            countryCode = country.Code;
        }

        return new CountryPackageDto
        {
            Id = pkg.Id,
            CountryId = pkg.CountryId,
            Name = pkg.Name,
            IsDefault = pkg.IsDefault,
            CountryNameEn = countryNameEn,
            CountryNameAr = countryNameAr,
            CountryCode = countryCode,
            MaidCost = pkg.MaidCost,
            MonthlyAccommodationCost = pkg.MonthlyAccommodationCost,
            VisaCost = pkg.VisaCost,
            EmploymentVisaCost = pkg.EmploymentVisaCost,
            ResidenceVisaCost = pkg.ResidenceVisaCost,
            MedicalCost = pkg.MedicalCost,
            TransportationCost = pkg.TransportationCost,
            TicketCost = pkg.TicketCost,
            InsuranceCost = pkg.InsuranceCost,
            EmiratesIdCost = pkg.EmiratesIdCost,
            OtherCosts = pkg.OtherCosts,
            TotalPackagePrice = pkg.TotalPackagePrice,
            SupplierCommission = pkg.SupplierCommission,
            SupplierCommissionType = pkg.SupplierCommissionType.ToString(),
            DefaultGuaranteePeriod = pkg.DefaultGuaranteePeriod.ToString(),
            Currency = pkg.Currency,
            EffectiveFrom = pkg.EffectiveFrom.ToString("yyyy-MM-dd"),
            EffectiveTo = pkg.EffectiveTo?.ToString("yyyy-MM-dd"),
            IsActive = pkg.IsActive,
            Notes = pkg.Notes,
            CreatedAt = pkg.CreatedAt,
            UpdatedAt = pkg.UpdatedAt,
        };
    }
}
