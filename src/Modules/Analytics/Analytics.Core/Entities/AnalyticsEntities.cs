using SaasKit.SharedKernel.Entities;

namespace Analytics.Core.Entities;

public class PageView : TenantScopedEntity
{
    public string Url { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string SessionId { get; set; } = string.Empty;
    public string? Referrer { get; set; }
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Device { get; set; }
    public string? Browser { get; set; }
    public int? DurationMs { get; set; }
}

public class AnalyticsEvent : TenantScopedEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Properties { get; set; } // JSONB
    public Guid? UserId { get; set; }
    public string? SessionId { get; set; }
}

public class AnalyticsSession : TenantScopedEntity
{
    public string SessionId { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? EndedAt { get; set; }
    public int PageViewCount { get; set; }
    public int EventCount { get; set; }
    public string? FirstPageUrl { get; set; }
    public string? LastPageUrl { get; set; }
    public string? Referrer { get; set; }
    public string? UserAgent { get; set; }
}

public class DailyStats : TenantScopedEntity
{
    public DateOnly Date { get; set; }
    public int PageViews { get; set; }
    public int UniqueVisitors { get; set; }
    public int Sessions { get; set; }
    public int Events { get; set; }
    public int AvgSessionDurationMs { get; set; }
    public decimal BounceRate { get; set; }
}
