using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SaasKit.Infrastructure.Api;
using SaasKit.Infrastructure.Persistence;
using SaasKit.SharedKernel.Api;
using SaasKit.SharedKernel.Models;
using Subscription.Contracts;
using Subscription.Contracts.DTOs;
using Subscription.Core.Entities;

namespace Subscription.Core.Services;

/// <summary>
/// Service for managing subscription plans.
/// </summary>
public class PlanService : IPlanService
{
    private readonly AppDbContext _db;

    private static readonly Dictionary<string, Expression<Func<Plan, object>>> PlanFilters = new()
    {
        ["isActive"] = x => x.IsActive,
        ["isDefault"] = x => x.IsDefault,
        ["name"] = x => x.Name,
        ["slug"] = x => x.Slug
    };

    private static readonly Dictionary<string, Expression<Func<Plan, object>>> PlanSortable = new()
    {
        ["displayOrder"] = x => x.DisplayOrder,
        ["name"] = x => x.Name,
        ["createdAt"] = x => x.CreatedAt
    };

    public PlanService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedList<PlanDto>> GetPlansAsync(QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<Plan>()
            .AsNoTracking()
            .Include(x => x.Prices.Where(p => p.IsActive))
            .Include(x => x.Features.OrderBy(f => f.DisplayOrder))
            .ApplyFilters(qp.Filters, PlanFilters)
            .ApplySort(qp.GetSortFields(), PlanSortable);

        var pagedPlans = await query.ToPagedListAsync(qp, ct);

        return new PagedList<PlanDto>(
            pagedPlans.Items.Select(MapToDto).ToList(),
            pagedPlans.TotalCount,
            pagedPlans.Page,
            pagedPlans.PageSize);
    }

    public async Task<Result<PlanDto>> GetPlanByIdAsync(Guid planId, CancellationToken ct = default)
    {
        var plan = await _db.Set<Plan>()
            .AsNoTracking()
            .Include(x => x.Prices.Where(p => p.IsActive))
            .Include(x => x.Features.OrderBy(f => f.DisplayOrder))
            .FirstOrDefaultAsync(x => x.Id == planId, ct);

        if (plan is null)
            return Result<PlanDto>.NotFound("Plan not found");

        return Result<PlanDto>.Success(MapToDto(plan));
    }

    public async Task<Result<PlanDto>> GetPlanBySlugAsync(string slug, CancellationToken ct = default)
    {
        var plan = await _db.Set<Plan>()
            .AsNoTracking()
            .Include(x => x.Prices.Where(p => p.IsActive))
            .Include(x => x.Features.OrderBy(f => f.DisplayOrder))
            .FirstOrDefaultAsync(x => x.Slug == slug, ct);

        if (plan is null)
            return Result<PlanDto>.NotFound("Plan not found");

        return Result<PlanDto>.Success(MapToDto(plan));
    }

    public async Task<Result<PlanDto>> GetDefaultPlanAsync(CancellationToken ct = default)
    {
        var plan = await _db.Set<Plan>()
            .AsNoTracking()
            .Include(x => x.Prices.Where(p => p.IsActive))
            .Include(x => x.Features.OrderBy(f => f.DisplayOrder))
            .FirstOrDefaultAsync(x => x.IsDefault && x.IsActive, ct);

        if (plan is null)
            return Result<PlanDto>.NotFound("No default plan configured");

        return Result<PlanDto>.Success(MapToDto(plan));
    }

    private static PlanDto MapToDto(Plan plan) => new()
    {
        Id = plan.Id,
        Name = plan.Name,
        Slug = plan.Slug,
        Description = plan.Description,
        IsActive = plan.IsActive,
        IsDefault = plan.IsDefault,
        DisplayOrder = plan.DisplayOrder,
        CreatedAt = plan.CreatedAt,
        Prices = plan.Prices.Select(p => new PlanPriceDto
        {
            Id = p.Id,
            PlanId = p.PlanId,
            Amount = p.Amount,
            Currency = p.Currency,
            Interval = p.Interval,
            IntervalCount = p.IntervalCount,
            TrialDays = p.TrialDays,
            IsActive = p.IsActive
        }).ToList(),
        Features = plan.Features.Select(f => new PlanFeatureDto
        {
            Id = f.Id,
            Key = f.Key,
            Name = f.Name,
            Description = f.Description,
            ValueType = f.ValueType,
            BooleanValue = f.BooleanValue,
            NumericValue = f.NumericValue,
            IsUnlimited = f.IsUnlimited,
            DisplayOrder = f.DisplayOrder
        }).ToList()
    };
}
