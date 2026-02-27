using System.Linq.Expressions;
using System.Text.Json;
using Audit.Contracts;
using Audit.Core.Entities;
using Microsoft.EntityFrameworkCore;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Audit.Core.Services;

public class AuditService : IAuditService
{
    private readonly AppDbContext _db;

    public AuditService(AppDbContext db) => _db = db;

    public async Task<PagedList<AuditEventDto>> GetEventsAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var filters = new Dictionary<string, Expression<Func<AuditEvent, object>>> { ["name"] = x => x.EventName, ["createdAt"] = x => x.CreatedAt };
        var query = _db.Set<AuditEvent>().IgnoreQueryFilters().AsNoTracking().Where(x => x.TenantId == tenantId)
            .ApplyFilters(qp.Filters, filters)
            .ApplySort(qp.GetSortFields(), new Dictionary<string, Expression<Func<AuditEvent, object>>> { ["createdAt"] = x => x.CreatedAt });
        return await query.Select(x => new AuditEventDto(x.Id, x.EventName, x.Payload, x.UserId, x.CreatedAt)).ToPagedListAsync(qp, ct);
    }

    public async Task<PagedList<AuditLogDto>> GetLogsAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var filters = new Dictionary<string, Expression<Func<AuditLog, object>>> { ["action"] = x => x.Action, ["entityType"] = x => x.EntityType, ["createdAt"] = x => x.CreatedAt };
        var query = _db.Set<AuditLog>().IgnoreQueryFilters().AsNoTracking().Where(x => x.TenantId == tenantId)
            .ApplyFilters(qp.Filters, filters)
            .ApplySort(qp.GetSortFields(), new Dictionary<string, Expression<Func<AuditLog, object>>> { ["createdAt"] = x => x.CreatedAt });
        return await query.Select(x => new AuditLogDto(x.Id, x.Action, x.EntityType, x.EntityId, x.OldValues, x.NewValues, x.UserId, x.CreatedAt)).ToPagedListAsync(qp, ct);
    }

    public async Task RecordEventAsync(Guid tenantId, string eventName, object? payload, Guid? userId, string? ipAddress, CancellationToken ct = default)
    {
        _db.Set<AuditEvent>().Add(new AuditEvent { Id = Guid.NewGuid(), TenantId = tenantId, EventName = eventName, Payload = payload != null ? JsonSerializer.Serialize(payload) : null, UserId = userId, IpAddress = ipAddress });
        await _db.SaveChangesAsync(ct);
    }

    public async Task RecordLogAsync(Guid tenantId, string action, string entityType, Guid entityId, object? oldValues, object? newValues, Guid? userId, CancellationToken ct = default)
    {
        _db.Set<AuditLog>().Add(new AuditLog { Id = Guid.NewGuid(), TenantId = tenantId, Action = action, EntityType = entityType, EntityId = entityId, OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null, NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null, UserId = userId });
        await _db.SaveChangesAsync(ct);
    }
}
