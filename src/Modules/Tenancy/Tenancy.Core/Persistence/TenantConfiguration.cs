using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenancy.Core.Entities;

namespace Tenancy.Core.Persistence;

/// <summary>
/// EF Core configuration for Tenant entity.
/// </summary>
public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.LogoUrl)
            .HasMaxLength(500);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.Website)
            .HasMaxLength(500);

        builder.Property(x => x.Settings)
            .HasColumnType("jsonb");

        // Unique constraint on slug
        builder.HasIndex(x => x.Slug)
            .IsUnique()
            .HasDatabaseName("ix_tenants_slug");

        // Index for status queries
        builder.HasIndex(x => x.Status)
            .HasDatabaseName("ix_tenants_status");

        // Index for created at
        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("ix_tenants_created_at");

        // Self-referential relationship for hierarchy
        builder.HasOne(x => x.ParentTenant)
            .WithMany(x => x.ChildTenants)
            .HasForeignKey(x => x.ParentTenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship to tenant type
        builder.HasOne(x => x.TenantType)
            .WithMany(x => x.Tenants)
            .HasForeignKey(x => x.TenantTypeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
