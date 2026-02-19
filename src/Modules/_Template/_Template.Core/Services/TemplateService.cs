using System.Linq.Expressions;
using _Template.Contracts;
using _Template.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SaasKit.Infrastructure.Api;
using SaasKit.Infrastructure.Persistence;
using SaasKit.SharedKernel.Api;
using SaasKit.SharedKernel.Models;

namespace _Template.Core.Services;

/// <summary>
/// Implementation of ITemplateService demonstrating standard patterns.
/// </summary>
public class TemplateService : ITemplateService
{
    private readonly AppDbContext _db;
    private readonly ILogger<TemplateService> _logger;

    // Define filterable fields (field name -> property expression)
    private static readonly Dictionary<string, Expression<Func<TemplateEntity, object>>> Filters = new()
    {
        ["name"] = x => x.Name,
        ["isActive"] = x => x.IsActive
    };

    // Define sortable fields
    private static readonly Dictionary<string, Expression<Func<TemplateEntity, object>>> Sortable = new()
    {
        ["name"] = x => x.Name,
        ["createdAt"] = x => x.CreatedAt,
        ["displayOrder"] = x => x.DisplayOrder
    };

    public TemplateService(AppDbContext db, ILogger<TemplateService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedList<TemplateEntityDto>> GetEntitiesAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<TemplateEntity>()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ApplyFilters(qp.Filters, Filters)    // Apply filters from query string
            .ApplySort(qp.GetSortFields(), Sortable);  // Apply sorting

        return await query
            .Select(x => MapToDto(x))
            .ToPagedListAsync(qp, ct);  // Paginate results
    }

    public async Task<Result<TemplateEntityDto>> GetByIdAsync(Guid tenantId, Guid entityId, CancellationToken ct = default)
    {
        var entity = await _db.Set<TemplateEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == entityId && x.TenantId == tenantId, ct);

        if (entity is null)
            return Result<TemplateEntityDto>.NotFound("Entity not found");

        return Result<TemplateEntityDto>.Success(MapToDto(entity));
    }

    public async Task<Result<TemplateEntityDto>> CreateAsync(Guid tenantId, CreateTemplateEntityRequest request, CancellationToken ct = default)
    {
        // Check for duplicate name (example business rule)
        if (await _db.Set<TemplateEntity>().AnyAsync(x => x.TenantId == tenantId && x.Name == request.Name, ct))
            return Result<TemplateEntityDto>.Conflict($"Entity with name '{request.Name}' already exists");

        var entity = new TemplateEntity
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Description = request.Description,
            DisplayOrder = request.DisplayOrder,
            IsActive = true
        };

        _db.Set<TemplateEntity>().Add(entity);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created template entity {EntityId} in tenant {TenantId}", entity.Id, tenantId);

        return Result<TemplateEntityDto>.Success(MapToDto(entity));
    }

    public async Task<Result<TemplateEntityDto>> UpdateAsync(Guid tenantId, Guid entityId, UpdateTemplateEntityRequest request, CancellationToken ct = default)
    {
        var entity = await _db.Set<TemplateEntity>()
            .FirstOrDefaultAsync(x => x.Id == entityId && x.TenantId == tenantId, ct);

        if (entity is null)
            return Result<TemplateEntityDto>.NotFound("Entity not found");

        // Update only provided fields (patch semantics)
        if (request.Name is not null) entity.Name = request.Name;
        if (request.Description is not null) entity.Description = request.Description;
        if (request.IsActive.HasValue) entity.IsActive = request.IsActive.Value;
        if (request.DisplayOrder.HasValue) entity.DisplayOrder = request.DisplayOrder.Value;

        await _db.SaveChangesAsync(ct);

        return Result<TemplateEntityDto>.Success(MapToDto(entity));
    }

    public async Task<Result<bool>> DeleteAsync(Guid tenantId, Guid entityId, CancellationToken ct = default)
    {
        var entity = await _db.Set<TemplateEntity>()
            .FirstOrDefaultAsync(x => x.Id == entityId && x.TenantId == tenantId, ct);

        if (entity is null)
            return Result<bool>.NotFound("Entity not found");

        _db.Set<TemplateEntity>().Remove(entity);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted template entity {EntityId}", entityId);

        return Result<bool>.Success(true);
    }

    private static TemplateEntityDto MapToDto(TemplateEntity e) => new(e.Id, e.Name, e.Description, e.IsActive, e.DisplayOrder, e.CreatedAt);
}
