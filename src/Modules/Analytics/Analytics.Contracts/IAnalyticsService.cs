using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Analytics.Contracts;

public record PageViewDto(Guid Id, string Url, Guid? UserId, string SessionId, string? Referrer, DateTimeOffset CreatedAt);
public record AnalyticsEventDto(Guid Id, string Name, string? Properties, Guid? UserId, DateTimeOffset CreatedAt);
public record DailyStatsDto(DateOnly Date, int PageViews, int UniqueVisitors, int Sessions, int Events);
public record TrackEventRequest(string Name, Dictionary<string, object>? Properties, string? SessionId);
public record TrackPageViewRequest(string Url, string SessionId, string? Referrer, string? UserAgent);

public interface IAnalyticsService
{
    Task<PagedList<PageViewDto>> GetPageViewsAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);
    Task<PagedList<AnalyticsEventDto>> GetEventsAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);
    Task<IReadOnlyList<DailyStatsDto>> GetDailyStatsAsync(Guid tenantId, DateOnly from, DateOnly to, CancellationToken ct = default);
    Task TrackEventAsync(Guid tenantId, Guid? userId, TrackEventRequest request, CancellationToken ct = default);
    Task TrackPageViewAsync(Guid tenantId, Guid? userId, TrackPageViewRequest request, string? ipAddress, CancellationToken ct = default);
}
