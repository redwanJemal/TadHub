using Authorization.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization.Core.Persistence;

/// <summary>
/// EF Core configuration for Role entity.
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        // Unique constraint on name within tenant
        builder.HasIndex(x => new { x.TenantId, x.Name })
            .IsUnique()
            .HasDatabaseName("ix_roles_tenant_name");

        // Index for default role queries
        builder.HasIndex(x => x.IsDefault)
            .HasDatabaseName("ix_roles_is_default");

        // Index for tenant queries
        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_roles_tenant_id");

        // Foreign key to role template (optional)
        builder.HasOne(x => x.Template)
            .WithMany()
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

/// <summary>
/// EF Core configuration for RolePermission entity.
/// </summary>
public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions");

        builder.HasKey(x => x.Id);

        // Unique constraint on role-permission pair
        builder.HasIndex(x => new { x.RoleId, x.PermissionId })
            .IsUnique()
            .HasDatabaseName("ix_role_permissions_role_permission");

        // Relationships
        builder.HasOne(x => x.Role)
            .WithMany(x => x.Permissions)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Permission)
            .WithMany(x => x.RolePermissions)
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
