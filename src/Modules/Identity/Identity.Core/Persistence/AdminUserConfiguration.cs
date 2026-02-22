using Identity.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Core.Persistence;

/// <summary>
/// EF Core configuration for PlatformStaff entity.
/// </summary>
public class PlatformStaffConfiguration : IEntityTypeConfiguration<PlatformStaff>
{
    public void Configure(EntityTypeBuilder<PlatformStaff> builder)
    {
        builder.ToTable("platform_staff");

        builder.HasKey(x => x.Id);

        // Unique constraint on UserId (one staff record per user)
        builder.HasIndex(x => x.UserId)
            .IsUnique()
            .HasDatabaseName("ix_platform_staff_user_id");

        builder.Property(x => x.Role)
            .IsRequired()
            .HasMaxLength(50)
            .HasDefaultValue("admin");

        builder.Property(x => x.Department)
            .HasMaxLength(200);

        // Foreign key relationship
        builder.HasOne(x => x.User)
            .WithOne(x => x.PlatformStaff)
            .HasForeignKey<PlatformStaff>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
