using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Financial.Contracts;
using Financial.Contracts.DTOs;
using Financial.Core.Entities;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;

namespace Financial.Core.Services;

public class DiscountProgramService : IDiscountProgramService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<DiscountProgramService> _logger;

    private static readonly Dictionary<string, Expression<Func<DiscountProgram, object>>> FilterableFields = new()
    {
        ["type"] = x => x.Type,
        ["isActive"] = x => x.IsActive,
    };

    private static readonly Dictionary<string, Expression<Func<DiscountProgram, object>>> SortableFields = new()
    {
        ["name"] = x => x.Name,
        ["type"] = x => x.Type,
        ["discountPercentage"] = x => x.DiscountPercentage,
        ["createdAt"] = x => x.CreatedAt,
    };

    public DiscountProgramService(
        AppDbContext db,
        IClock clock,
        ICurrentUser currentUser,
        ILogger<DiscountProgramService> logger)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<PagedList<DiscountProgramListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<DiscountProgram>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .ApplyFilters(qp.Filters, FilterableFields)
            .ApplySort(qp.GetSortFields(), SortableFields);

        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var searchLower = qp.Search.ToLower();
            query = query.Where(x =>
                x.Name.ToLower().Contains(searchLower) ||
                (x.NameAr != null && x.NameAr.ToLower().Contains(searchLower)));
        }

        return await query
            .Select(x => MapToListDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<DiscountProgramDto>> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var program = await _db.Set<DiscountProgram>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (program is null)
            return Result<DiscountProgramDto>.NotFound($"Discount program with ID {id} not found");

        return Result<DiscountProgramDto>.Success(MapToDto(program));
    }

    public async Task<Result<DiscountProgramDto>> CreateAsync(Guid tenantId, CreateDiscountProgramRequest request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<DiscountType>(request.Type, ignoreCase: true, out var discountType))
            return Result<DiscountProgramDto>.ValidationError($"Invalid discount type '{request.Type}'");

        var program = new DiscountProgram
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            NameAr = request.NameAr,
            Type = discountType,
            DiscountPercentage = request.DiscountPercentage,
            MaxDiscountAmount = request.MaxDiscountAmount,
            IsActive = request.IsActive,
            ValidFrom = request.ValidFrom,
            ValidTo = request.ValidTo,
            Description = request.Description,
            CreatedBy = _currentUser.UserId,
        };

        _db.Set<DiscountProgram>().Add(program);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created discount program {Name} ({Type})", request.Name, request.Type);

        return Result<DiscountProgramDto>.Success(MapToDto(program));
    }

    public async Task<Result<DiscountProgramDto>> UpdateAsync(Guid tenantId, Guid id, UpdateDiscountProgramRequest request, CancellationToken ct = default)
    {
        var program = await _db.Set<DiscountProgram>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (program is null)
            return Result<DiscountProgramDto>.NotFound($"Discount program with ID {id} not found");

        if (request.Name is not null) program.Name = request.Name;
        if (request.NameAr is not null) program.NameAr = request.NameAr;
        if (request.DiscountPercentage.HasValue) program.DiscountPercentage = request.DiscountPercentage.Value;
        if (request.MaxDiscountAmount.HasValue) program.MaxDiscountAmount = request.MaxDiscountAmount.Value;
        if (request.IsActive.HasValue) program.IsActive = request.IsActive.Value;
        if (request.ValidFrom.HasValue) program.ValidFrom = request.ValidFrom.Value;
        if (request.ValidTo.HasValue) program.ValidTo = request.ValidTo.Value;
        if (request.Description is not null) program.Description = request.Description;

        program.UpdatedBy = _currentUser.UserId;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated discount program {ProgramId}", id);

        return Result<DiscountProgramDto>.Success(MapToDto(program));
    }

    public async Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var program = await _db.Set<DiscountProgram>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (program is null)
            return Result.NotFound($"Discount program with ID {id} not found");

        program.MarkAsDeleted(_currentUser.UserId);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Soft-deleted discount program {ProgramId}", id);

        return Result.Success();
    }

    public async Task<Result<decimal>> CalculateDiscountAsync(Guid tenantId, Guid programId, decimal baseAmount, CancellationToken ct = default)
    {
        var program = await _db.Set<DiscountProgram>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == programId && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (program is null)
            return Result<decimal>.NotFound("Discount program not found");

        if (!program.IsActive)
            return Result<decimal>.ValidationError("Discount program is not active");

        var today = _clock.Today;
        if (program.ValidFrom.HasValue && today < program.ValidFrom.Value)
            return Result<decimal>.ValidationError("Discount program is not yet valid");
        if (program.ValidTo.HasValue && today > program.ValidTo.Value)
            return Result<decimal>.ValidationError("Discount program has expired");

        var discount = Math.Round(baseAmount * program.DiscountPercentage / 100m, 2);
        if (program.MaxDiscountAmount.HasValue && discount > program.MaxDiscountAmount.Value)
            discount = program.MaxDiscountAmount.Value;

        return Result<decimal>.Success(discount);
    }

    #region Mapping

    private static DiscountProgramDto MapToDto(DiscountProgram p)
    {
        return new DiscountProgramDto
        {
            Id = p.Id,
            TenantId = p.TenantId,
            Name = p.Name,
            NameAr = p.NameAr,
            Type = p.Type.ToString(),
            DiscountPercentage = p.DiscountPercentage,
            MaxDiscountAmount = p.MaxDiscountAmount,
            IsActive = p.IsActive,
            ValidFrom = p.ValidFrom,
            ValidTo = p.ValidTo,
            Description = p.Description,
            CreatedBy = p.CreatedBy,
            UpdatedBy = p.UpdatedBy,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
        };
    }

    private static DiscountProgramListDto MapToListDto(DiscountProgram p)
    {
        return new DiscountProgramListDto
        {
            Id = p.Id,
            Name = p.Name,
            NameAr = p.NameAr,
            Type = p.Type.ToString(),
            DiscountPercentage = p.DiscountPercentage,
            MaxDiscountAmount = p.MaxDiscountAmount,
            IsActive = p.IsActive,
            ValidFrom = p.ValidFrom,
            ValidTo = p.ValidTo,
            CreatedAt = p.CreatedAt,
        };
    }

    #endregion
}
