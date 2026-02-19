using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Subscription.Core.Entities;

namespace Subscription.Core.Persistence;

public class TenantSubscriptionConfiguration : IEntityTypeConfiguration<TenantSubscription>
{
    public void Configure(EntityTypeBuilder<TenantSubscription> builder)
    {
        builder.ToTable("tenant_subscriptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.StripeSubscriptionId)
            .HasMaxLength(256);

        builder.Property(x => x.StripeCustomerId)
            .HasMaxLength(256);

        builder.HasIndex(x => x.TenantId)
            .IsUnique()
            .HasDatabaseName("ix_tenant_subscriptions_tenant");

        builder.HasIndex(x => x.StripeSubscriptionId)
            .HasDatabaseName("ix_tenant_subscriptions_stripe_id");

        builder.HasOne(x => x.Plan)
            .WithMany()
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PlanPrice)
            .WithMany()
            .HasForeignKey(x => x.PlanPriceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Products)
            .WithOne(x => x.TenantSubscription)
            .HasForeignKey(x => x.TenantSubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TenantSubscriptionProductConfiguration : IEntityTypeConfiguration<TenantSubscriptionProduct>
{
    public void Configure(EntityTypeBuilder<TenantSubscriptionProduct> builder)
    {
        builder.ToTable("tenant_subscription_products");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductName)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.StripeSubscriptionItemId)
            .HasMaxLength(256);

        builder.HasIndex(x => x.TenantSubscriptionId)
            .HasDatabaseName("ix_tenant_sub_products_subscription");

        builder.HasMany(x => x.Prices)
            .WithOne(x => x.TenantSubscriptionProduct)
            .HasForeignKey(x => x.TenantSubscriptionProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TenantSubscriptionPriceConfiguration : IEntityTypeConfiguration<TenantSubscriptionPrice>
{
    public void Configure(EntityTypeBuilder<TenantSubscriptionPrice> builder)
    {
        builder.ToTable("tenant_subscription_prices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StripePriceId)
            .HasMaxLength(256);

        builder.Property(x => x.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(x => x.Interval)
            .HasMaxLength(16)
            .IsRequired();
    }
}

public class TenantUsageRecordConfiguration : IEntityTypeConfiguration<TenantUsageRecord>
{
    public void Configure(EntityTypeBuilder<TenantUsageRecord> builder)
    {
        builder.ToTable("tenant_usage_records");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.MetricKey)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.StripeUsageRecordId)
            .HasMaxLength(256);

        builder.HasIndex(x => new { x.TenantId, x.MetricKey, x.PeriodStart })
            .HasDatabaseName("ix_tenant_usage_records_tenant_metric_period");

        builder.HasOne(x => x.TenantSubscription)
            .WithMany()
            .HasForeignKey(x => x.TenantSubscriptionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CheckoutSessionConfiguration : IEntityTypeConfiguration<CheckoutSession>
{
    public void Configure(EntityTypeBuilder<CheckoutSession> builder)
    {
        builder.ToTable("checkout_sessions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StripeSessionId)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Url)
            .HasMaxLength(2048);

        builder.HasIndex(x => x.StripeSessionId)
            .IsUnique()
            .HasDatabaseName("ix_checkout_sessions_stripe_id");

        builder.HasIndex(x => new { x.TenantId, x.Status })
            .HasDatabaseName("ix_checkout_sessions_tenant_status");

        builder.HasOne(x => x.Plan)
            .WithMany()
            .HasForeignKey(x => x.PlanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PlanPrice)
            .WithMany()
            .HasForeignKey(x => x.PlanPriceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
