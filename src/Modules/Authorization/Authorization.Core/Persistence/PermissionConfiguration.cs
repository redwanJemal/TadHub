using Authorization.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization.Core.Persistence;

/// <summary>
/// EF Core configuration for Permission entity.
/// </summary>
public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Module)
            .IsRequired()
            .HasMaxLength(50);

        // Unique constraint on name
        builder.HasIndex(x => x.Name)
            .IsUnique()
            .HasDatabaseName("ix_permissions_name");

        // Index for module queries
        builder.HasIndex(x => x.Module)
            .HasDatabaseName("ix_permissions_module");
    }
}
