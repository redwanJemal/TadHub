using Analytics.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Analytics.Core.Persistence;

public class PageViewConfiguration : IEntityTypeConfiguration<PageView>
{
    public void Configure(EntityTypeBuilder<PageView> builder)
    {
        builder.ToTable("page_views");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Url).HasMaxLength(2048).IsRequired();
        builder.Property(x => x.SessionId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Referrer).HasMaxLength(2048);
        builder.Property(x => x.UserAgent).HasMaxLength(512);
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.Country).HasMaxLength(2);
        builder.Property(x => x.City).HasMaxLength(128);
        builder.Property(x => x.Device).HasMaxLength(32);
        builder.Property(x => x.Browser).HasMaxLength(64);
        builder.HasIndex(x => new { x.TenantId, x.CreatedAt }).HasDatabaseName("ix_page_views_tenant_created");
        builder.HasIndex(x => new { x.TenantId, x.Url, x.CreatedAt }).HasDatabaseName("ix_page_views_tenant_url_created");
    }
}

public class AnalyticsEventConfiguration : IEntityTypeConfiguration<AnalyticsEvent>
{
    public void Configure(EntityTypeBuilder<AnalyticsEvent> builder)
    {
        builder.ToTable("analytics_events");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Properties).HasColumnType("jsonb");
        builder.Property(x => x.SessionId).HasMaxLength(64);
        builder.HasIndex(x => new { x.TenantId, x.Name, x.CreatedAt }).HasDatabaseName("ix_analytics_events_tenant_name_created");
    }
}

public class AnalyticsSessionConfiguration : IEntityTypeConfiguration<AnalyticsSession>
{
    public void Configure(EntityTypeBuilder<AnalyticsSession> builder)
    {
        builder.ToTable("analytics_sessions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SessionId).HasMaxLength(64).IsRequired();
        builder.Property(x => x.FirstPageUrl).HasMaxLength(2048);
        builder.Property(x => x.LastPageUrl).HasMaxLength(2048);
        builder.Property(x => x.Referrer).HasMaxLength(2048);
        builder.Property(x => x.UserAgent).HasMaxLength(512);
        builder.HasIndex(x => new { x.TenantId, x.SessionId }).IsUnique().HasDatabaseName("ix_analytics_sessions_tenant_session");
    }
}

public class DailyStatsConfiguration : IEntityTypeConfiguration<DailyStats>
{
    public void Configure(EntityTypeBuilder<DailyStats> builder)
    {
        builder.ToTable("daily_stats");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.TenantId, x.Date }).IsUnique().HasDatabaseName("ix_daily_stats_tenant_date");
    }
}
