using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Subscription.Core.Entities;

namespace Subscription.Core.Persistence;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("plans");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1024);

        builder.Property(x => x.StripeProductId)
            .HasMaxLength(256);

        builder.HasIndex(x => x.Slug)
            .IsUnique()
            .HasDatabaseName("ix_plans_slug");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("ix_plans_active");

        builder.HasMany(x => x.Prices)
            .WithOne(x => x.Plan)
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Features)
            .WithOne(x => x.Plan)
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.UsageBasedPrices)
            .WithOne(x => x.Plan)
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PlanPriceConfiguration : IEntityTypeConfiguration<PlanPrice>
{
    public void Configure(EntityTypeBuilder<PlanPrice> builder)
    {
        builder.ToTable("plan_prices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.Interval)
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(x => x.StripePriceId)
            .HasMaxLength(256);

        builder.HasIndex(x => new { x.PlanId, x.Interval, x.Currency })
            .HasDatabaseName("ix_plan_prices_plan_interval_currency");
    }
}

public class PlanFeatureConfiguration : IEntityTypeConfiguration<PlanFeature>
{
    public void Configure(EntityTypeBuilder<PlanFeature> builder)
    {
        builder.ToTable("plan_features");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(512);

        builder.Property(x => x.ValueType)
            .HasMaxLength(16)
            .IsRequired();

        builder.HasIndex(x => new { x.PlanId, x.Key })
            .IsUnique()
            .HasDatabaseName("ix_plan_features_plan_key");
    }
}

public class PlanUsageBasedPriceConfiguration : IEntityTypeConfiguration<PlanUsageBasedPrice>
{
    public void Configure(EntityTypeBuilder<PlanUsageBasedPrice> builder)
    {
        builder.ToTable("plan_usage_based_prices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MetricKey)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.Unit)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.StripePriceId)
            .HasMaxLength(256);

        builder.HasIndex(x => new { x.PlanId, x.MetricKey })
            .IsUnique()
            .HasDatabaseName("ix_plan_usage_prices_plan_metric");
    }
}
