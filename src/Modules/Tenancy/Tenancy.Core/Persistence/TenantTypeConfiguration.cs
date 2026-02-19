using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenancy.Core.Entities;

namespace Tenancy.Core.Persistence;

/// <summary>
/// EF Core configuration for TenantType entity.
/// </summary>
public class TenantTypeConfiguration : IEntityTypeConfiguration<TenantType>
{
    public void Configure(EntityTypeBuilder<TenantType> builder)
    {
        builder.ToTable("tenant_types");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.HasIndex(x => x.Name)
            .IsUnique()
            .HasDatabaseName("ix_tenant_types_name");

        builder.HasIndex(x => x.DisplayOrder)
            .HasDatabaseName("ix_tenant_types_display_order");
    }
}

/// <summary>
/// EF Core configuration for TenantTypeRelationship entity.
/// </summary>
public class TenantTypeRelationshipConfiguration : IEntityTypeConfiguration<TenantTypeRelationship>
{
    public void Configure(EntityTypeBuilder<TenantTypeRelationship> builder)
    {
        builder.ToTable("tenant_type_relationships");

        builder.HasKey(x => x.Id);

        // Unique constraint on parent-child pair
        builder.HasIndex(x => new { x.ParentTypeId, x.ChildTypeId })
            .IsUnique()
            .HasDatabaseName("ix_tenant_type_relationships_parent_child");

        // Relationships
        builder.HasOne(x => x.ParentType)
            .WithMany(x => x.AllowedChildTypes)
            .HasForeignKey(x => x.ParentTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ChildType)
            .WithMany(x => x.ParentTypes)
            .HasForeignKey(x => x.ChildTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
