using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Core.Entities;

namespace Notification.Core.Persistence;

public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("notification_templates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.TitleEn)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.TitleAr)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.BodyEn)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(x => x.BodyAr)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(x => x.DefaultPriority)
            .HasMaxLength(16)
            .IsRequired()
            .HasDefaultValue("normal");

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(x => new { x.TenantId, x.EventType })
            .HasDatabaseName("ix_notification_templates_tenant_event")
            .IsUnique();
    }
}
