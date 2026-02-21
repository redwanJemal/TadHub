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
/// Service implementation for MoHRE job categories.
/// </summary>
public class JobCategoryService : IJobCategoryService
{
    private readonly AppDbContext _db;
    private readonly ILogger<JobCategoryService> _logger;

    // Filterable fields
    private static readonly Dictionary<string, Expression<Func<JobCategory, object>>> Filters = new()
    {
        ["moHRECode"] = x => x.MoHRECode,
        ["isActive"] = x => x.IsActive
    };

    // Sortable fields
    private static readonly Dictionary<string, Expression<Func<JobCategory, object>>> Sortable = new()
    {
        ["moHRECode"] = x => x.MoHRECode,
        ["displayOrder"] = x => x.DisplayOrder,
        ["createdAt"] = x => x.CreatedAt
    };

    public JobCategoryService(AppDbContext db, ILogger<JobCategoryService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<List<JobCategoryDto>> GetAllAsync(CancellationToken ct = default)
    {
        var categories = await _db.Set<JobCategory>()
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name.En)
            .ToListAsync(ct);

        return categories.Select(MapToDto).ToList();
    }

    public async Task<PagedList<JobCategoryDto>> ListAsync(QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<JobCategory>()
            .AsNoTracking()
            .ApplyFilters(qp.Filters, Filters)
            .ApplySort(qp.GetSortFields(), Sortable);

        return await query
            .Select(x => MapToDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<JobCategoryDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _db.Set<JobCategory>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (category is null)
            return Result<JobCategoryDto>.NotFound("Job category not found");

        return Result<JobCategoryDto>.Success(MapToDto(category));
    }

    public async Task<Result<JobCategoryDto>> GetByCodeAsync(string moHRECode, CancellationToken ct = default)
    {
        var category = await _db.Set<JobCategory>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MoHRECode == moHRECode.ToUpperInvariant(), ct);

        if (category is null)
            return Result<JobCategoryDto>.NotFound($"Job category with code '{moHRECode}' not found");

        return Result<JobCategoryDto>.Success(MapToDto(category));
    }

    public async Task<List<JobCategoryRefDto>> GetReferencesAsync(CancellationToken ct = default)
    {
        var categories = await _db.Set<JobCategory>()
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new JobCategoryRefDto
            {
                Id = x.Id,
                MoHRECode = x.MoHRECode,
                NameEn = x.Name.En,
                NameAr = x.Name.Ar
            })
            .ToListAsync(ct);

        return categories;
    }

    private static JobCategoryDto MapToDto(JobCategory c) => new()
    {
        Id = c.Id,
        MoHRECode = c.MoHRECode,
        NameEn = c.Name.En,
        NameAr = c.Name.Ar,
        IsActive = c.IsActive,
        DisplayOrder = c.DisplayOrder
    };
}
