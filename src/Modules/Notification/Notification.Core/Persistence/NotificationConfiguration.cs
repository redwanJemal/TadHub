using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Notification.Core.Persistence;

public class NotificationConfiguration : IEntityTypeConfiguration<Entities.Notification>
{
    public void Configure(EntityTypeBuilder<Entities.Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Body)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasMaxLength(32)
            .IsRequired()
            .HasDefaultValue("info");

        builder.Property(x => x.Link)
            .HasMaxLength(2048);

        builder.Property(x => x.IsRead)
            .HasDefaultValue(false);

        // Index for querying user's notifications
        builder.HasIndex(x => new { x.TenantId, x.UserId, x.IsRead, x.CreatedAt })
            .HasDatabaseName("ix_notifications_tenant_user_read_created");

        // Index for unread count queries
        builder.HasIndex(x => new { x.TenantId, x.UserId, x.IsRead })
            .HasDatabaseName("ix_notifications_tenant_user_unread")
            .HasFilter("is_read = false");
    }
}
