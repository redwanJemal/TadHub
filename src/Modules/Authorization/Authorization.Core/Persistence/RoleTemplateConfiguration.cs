using Authorization.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization.Core.Persistence;

/// <summary>
/// EF Core configuration for RoleTemplate entity.
/// </summary>
public class RoleTemplateConfiguration : IEntityTypeConfiguration<RoleTemplate>
{
    public void Configure(EntityTypeBuilder<RoleTemplate> builder)
    {
        builder.ToTable("role_templates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        // Unique constraint on name
        builder.HasIndex(x => x.Name)
            .IsUnique()
            .HasDatabaseName("ix_role_templates_name");
    }
}

/// <summary>
/// EF Core configuration for RoleTemplatePermission entity.
/// </summary>
public class RoleTemplatePermissionConfiguration : IEntityTypeConfiguration<RoleTemplatePermission>
{
    public void Configure(EntityTypeBuilder<RoleTemplatePermission> builder)
    {
        builder.ToTable("role_template_permissions");

        builder.HasKey(x => x.Id);

        // Composite unique constraint on template-permission pair
        builder.HasIndex(x => new { x.TemplateId, x.PermissionId })
            .IsUnique()
            .HasDatabaseName("ix_role_template_permissions_template_permission");

        // Relationships
        builder.HasOne(x => x.Template)
            .WithMany(x => x.Permissions)
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Permission)
            .WithMany()
            .HasForeignKey(x => x.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
