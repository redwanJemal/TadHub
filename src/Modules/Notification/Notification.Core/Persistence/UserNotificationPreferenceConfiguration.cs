using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Core.Entities;

namespace Notification.Core.Persistence;

public class UserNotificationPreferenceConfiguration : IEntityTypeConfiguration<UserNotificationPreference>
{
    public void Configure(EntityTypeBuilder<UserNotificationPreference> builder)
    {
        builder.ToTable("user_notification_preferences");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Channels)
            .HasMaxLength(256)
            .IsRequired()
            .HasDefaultValue("in_app");

        builder.HasIndex(x => new { x.TenantId, x.UserId, x.EventType })
            .HasDatabaseName("ix_user_notification_prefs_tenant_user_event")
            .IsUnique();
    }
}
