using System.Linq.Expressions;
using System.Text.Json;
using Analytics.Contracts;
using Analytics.Core.Entities;
using Microsoft.EntityFrameworkCore;
using SaasKit.Infrastructure.Api;
using SaasKit.Infrastructure.Persistence;
using SaasKit.SharedKernel.Api;
using SaasKit.SharedKernel.Models;

namespace Analytics.Core.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly AppDbContext _db;

    public AnalyticsService(AppDbContext db) => _db = db;

    public async Task<PagedList<PageViewDto>> GetPageViewsAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var filters = new Dictionary<string, Expression<Func<PageView, object>>> { ["url"] = x => x.Url, ["createdAt"] = x => x.CreatedAt };
        var query = _db.Set<PageView>().AsNoTracking().Where(x => x.TenantId == tenantId)
            .ApplyFilters(qp.Filters, filters)
            .ApplySort(qp.GetSortFields(), new Dictionary<string, Expression<Func<PageView, object>>> { ["createdAt"] = x => x.CreatedAt });
        return await query.Select(x => new PageViewDto(x.Id, x.Url, x.UserId, x.SessionId, x.Referrer, x.CreatedAt)).ToPagedListAsync(qp, ct);
    }

    public async Task<PagedList<AnalyticsEventDto>> GetEventsAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var filters = new Dictionary<string, Expression<Func<AnalyticsEvent, object>>> { ["name"] = x => x.Name, ["createdAt"] = x => x.CreatedAt };
        var query = _db.Set<AnalyticsEvent>().AsNoTracking().Where(x => x.TenantId == tenantId)
            .ApplyFilters(qp.Filters, filters)
            .ApplySort(qp.GetSortFields(), new Dictionary<string, Expression<Func<AnalyticsEvent, object>>> { ["createdAt"] = x => x.CreatedAt });
        return await query.Select(x => new AnalyticsEventDto(x.Id, x.Name, x.Properties, x.UserId, x.CreatedAt)).ToPagedListAsync(qp, ct);
    }

    public async Task<IReadOnlyList<DailyStatsDto>> GetDailyStatsAsync(Guid tenantId, DateOnly from, DateOnly to, CancellationToken ct = default)
    {
        return await _db.Set<DailyStats>().AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Date >= from && x.Date <= to)
            .OrderBy(x => x.Date)
            .Select(x => new DailyStatsDto(x.Date, x.PageViews, x.UniqueVisitors, x.Sessions, x.Events))
            .ToListAsync(ct);
    }

    public async Task TrackEventAsync(Guid tenantId, Guid? userId, TrackEventRequest request, CancellationToken ct = default)
    {
        _db.Set<AnalyticsEvent>().Add(new AnalyticsEvent
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Name = request.Name,
            Properties = request.Properties != null ? JsonSerializer.Serialize(request.Properties) : null,
            UserId = userId,
            SessionId = request.SessionId
        });
        await _db.SaveChangesAsync(ct);
    }

    public async Task TrackPageViewAsync(Guid tenantId, Guid? userId, TrackPageViewRequest request, string? ipAddress, CancellationToken ct = default)
    {
        _db.Set<PageView>().Add(new PageView
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Url = request.Url,
            UserId = userId,
            SessionId = request.SessionId,
            Referrer = request.Referrer,
            UserAgent = request.UserAgent,
            IpAddress = ipAddress
        });
        await _db.SaveChangesAsync(ct);
    }
}
