using Audit.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Audit.Core.Persistence;

public class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("audit_events");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Payload).HasColumnType("jsonb");
        builder.Property(x => x.Metadata).HasColumnType("jsonb");
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.HasIndex(x => new { x.TenantId, x.EventName, x.CreatedAt }).HasDatabaseName("ix_audit_events_tenant_name_created");
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasMaxLength(32).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(128).IsRequired();
        builder.Property(x => x.OldValues).HasColumnType("jsonb");
        builder.Property(x => x.NewValues).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.TenantId, x.EntityType, x.Action, x.CreatedAt }).HasDatabaseName("ix_audit_logs_tenant_entity_action");
    }
}

public class WebhookConfiguration : IEntityTypeConfiguration<Webhook>
{
    public void Configure(EntityTypeBuilder<Webhook> builder)
    {
        builder.ToTable("webhooks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Url).HasMaxLength(2048).IsRequired();
        builder.Property(x => x.Events).HasColumnType("jsonb");
        builder.Property(x => x.Secret).HasMaxLength(128).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.IsActive }).HasDatabaseName("ix_webhooks_tenant_active");
        builder.HasMany(x => x.Deliveries).WithOne(x => x.Webhook).HasForeignKey(x => x.WebhookId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class WebhookDeliveryConfiguration : IEntityTypeConfiguration<WebhookDelivery>
{
    public void Configure(EntityTypeBuilder<WebhookDelivery> builder)
    {
        builder.ToTable("webhook_deliveries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Payload).HasColumnType("jsonb");
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.ErrorMessage).HasMaxLength(2048);
        builder.HasIndex(x => new { x.WebhookId, x.Status, x.CreatedAt }).HasDatabaseName("ix_webhook_deliveries_webhook_status");
    }
}
