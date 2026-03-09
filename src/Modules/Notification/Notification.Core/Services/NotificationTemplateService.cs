using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Notification.Contracts;
using Notification.Contracts.DTOs;
using Notification.Core.Entities;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Notification.Core.Services;

public sealed class NotificationTemplateService : INotificationTemplateService
{
    private readonly AppDbContext _db;
    private readonly ILogger<NotificationTemplateService> _logger;

    private static readonly Dictionary<string, Expression<Func<NotificationTemplate, object>>> FilterableFields = new()
    {
        ["eventType"] = x => x.EventType,
        ["isActive"] = x => x.IsActive
    };

    private static readonly Dictionary<string, Expression<Func<NotificationTemplate, object>>> SortableFields = new()
    {
        ["eventType"] = x => x.EventType,
        ["createdAt"] = x => x.CreatedAt
    };

    public NotificationTemplateService(AppDbContext db, ILogger<NotificationTemplateService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedList<NotificationTemplateListDto>> ListAsync(
        Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<NotificationTemplate>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted)
            .ApplyFilters(qp.Filters, FilterableFields)
            .ApplySort(qp.GetSortFields(), SortableFields);

        return await query
            .Select(x => new NotificationTemplateListDto
            {
                Id = x.Id,
                EventType = x.EventType,
                TitleEn = x.TitleEn,
                TitleAr = x.TitleAr,
                DefaultPriority = x.DefaultPriority,
                IsActive = x.IsActive
            })
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<NotificationTemplateDto>> GetByIdAsync(
        Guid tenantId, Guid templateId, CancellationToken ct = default)
    {
        var template = await _db.Set<NotificationTemplate>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == templateId && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (template is null)
            return Result<NotificationTemplateDto>.NotFound("Template not found");

        return Result<NotificationTemplateDto>.Success(MapToDto(template));
    }

    public async Task<Result<NotificationTemplateDto>> GetByEventTypeAsync(
        Guid tenantId, string eventType, CancellationToken ct = default)
    {
        var template = await _db.Set<NotificationTemplate>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.EventType == eventType && !x.IsDeleted, ct);

        if (template is null)
            return Result<NotificationTemplateDto>.NotFound("Template not found");

        return Result<NotificationTemplateDto>.Success(MapToDto(template));
    }

    public async Task<Result<NotificationTemplateDto>> CreateAsync(
        Guid tenantId, CreateNotificationTemplateRequest request, CancellationToken ct = default)
    {
        var exists = await _db.Set<NotificationTemplate>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.TenantId == tenantId && x.EventType == request.EventType && !x.IsDeleted, ct);

        if (exists)
            return Result<NotificationTemplateDto>.Conflict($"Template for event type '{request.EventType}' already exists");

        var template = new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EventType = request.EventType,
            TitleEn = request.TitleEn,
            TitleAr = request.TitleAr,
            BodyEn = request.BodyEn,
            BodyAr = request.BodyAr,
            DefaultPriority = request.DefaultPriority,
            IsActive = true
        };

        _db.Set<NotificationTemplate>().Add(template);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created notification template {TemplateId} for event {EventType} in tenant {TenantId}",
            template.Id, request.EventType, tenantId);

        return Result<NotificationTemplateDto>.Success(MapToDto(template));
    }

    public async Task<Result<NotificationTemplateDto>> UpdateAsync(
        Guid tenantId, Guid templateId, UpdateNotificationTemplateRequest request, CancellationToken ct = default)
    {
        var template = await _db.Set<NotificationTemplate>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == templateId && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (template is null)
            return Result<NotificationTemplateDto>.NotFound("Template not found");

        if (request.TitleEn is not null) template.TitleEn = request.TitleEn;
        if (request.TitleAr is not null) template.TitleAr = request.TitleAr;
        if (request.BodyEn is not null) template.BodyEn = request.BodyEn;
        if (request.BodyAr is not null) template.BodyAr = request.BodyAr;
        if (request.DefaultPriority is not null) template.DefaultPriority = request.DefaultPriority;
        if (request.IsActive.HasValue) template.IsActive = request.IsActive.Value;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated notification template {TemplateId} in tenant {TenantId}",
            templateId, tenantId);

        return Result<NotificationTemplateDto>.Success(MapToDto(template));
    }

    public async Task<Result<bool>> DeleteAsync(
        Guid tenantId, Guid templateId, CancellationToken ct = default)
    {
        var template = await _db.Set<NotificationTemplate>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == templateId && x.TenantId == tenantId && !x.IsDeleted, ct);

        if (template is null)
            return Result<bool>.NotFound("Template not found");

        template.IsDeleted = true;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Deleted notification template {TemplateId} in tenant {TenantId}",
            templateId, tenantId);

        return Result<bool>.Success(true);
    }

    private static NotificationTemplateDto MapToDto(NotificationTemplate t) => new()
    {
        Id = t.Id,
        TenantId = t.TenantId,
        EventType = t.EventType,
        TitleEn = t.TitleEn,
        TitleAr = t.TitleAr,
        BodyEn = t.BodyEn,
        BodyAr = t.BodyAr,
        DefaultPriority = t.DefaultPriority,
        IsActive = t.IsActive,
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt
    };
}
