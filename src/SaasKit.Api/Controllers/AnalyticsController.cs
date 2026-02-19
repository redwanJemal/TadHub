using Analytics.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaasKit.Api.Filters;
using SaasKit.Infrastructure.Auth;
using SaasKit.SharedKernel.Api;
using SaasKit.SharedKernel.Interfaces;

namespace SaasKit.Api.Controllers;

[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/analytics")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ICurrentUser _currentUser;

    public AnalyticsController(IAnalyticsService analyticsService, ICurrentUser currentUser)
    {
        _analyticsService = analyticsService;
        _currentUser = currentUser;
    }

    [HttpGet("pageviews")]
    [HasPermission("analytics.view")]
    public async Task<IActionResult> GetPageViews(Guid tenantId, [FromQuery] QueryParameters qp, CancellationToken ct)
        => Ok(await _analyticsService.GetPageViewsAsync(tenantId, qp, ct));

    [HttpGet("events")]
    [HasPermission("analytics.view")]
    public async Task<IActionResult> GetEvents(Guid tenantId, [FromQuery] QueryParameters qp, CancellationToken ct)
        => Ok(await _analyticsService.GetEventsAsync(tenantId, qp, ct));

    [HttpGet("daily")]
    [HasPermission("analytics.view")]
    public async Task<IActionResult> GetDailyStats(Guid tenantId, [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken ct)
    {
        var toDate = to ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var fromDate = from ?? toDate.AddDays(-30);
        return Ok(await _analyticsService.GetDailyStatsAsync(tenantId, fromDate, toDate, ct));
    }

    [HttpPost("track")]
    public async Task<IActionResult> TrackEvent(Guid tenantId, [FromBody] TrackEventRequest request, CancellationToken ct)
    {
        await _analyticsService.TrackEventAsync(tenantId, _currentUser.UserId, request, ct);
        return NoContent();
    }

    [HttpPost("pageview")]
    public async Task<IActionResult> TrackPageView(Guid tenantId, [FromBody] TrackPageViewRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _analyticsService.TrackPageViewAsync(tenantId, _currentUser.UserId, request, ip, ct);
        return NoContent();
    }
}
