using Identity.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Core.Persistence;

/// <summary>
/// EF Core configuration for AdminUser entity.
/// </summary>
public class AdminUserConfiguration : IEntityTypeConfiguration<AdminUser>
{
    public void Configure(EntityTypeBuilder<AdminUser> builder)
    {
        builder.ToTable("admin_users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.IsSuperAdmin)
            .HasDefaultValue(false);

        // Unique constraint on UserId (one admin record per user)
        builder.HasIndex(x => x.UserId)
            .IsUnique()
            .HasDatabaseName("ix_admin_users_user_id");

        // Foreign key relationship
        builder.HasOne(x => x.User)
            .WithOne(x => x.AdminUser)
            .HasForeignKey<AdminUser>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
