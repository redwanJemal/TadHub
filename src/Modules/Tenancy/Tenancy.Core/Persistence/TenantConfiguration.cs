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

        builder.Property(x => x.NameAr)
            .HasMaxLength(255);

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

        #region Tadbeer Agency Fields

        builder.Property(x => x.TadbeerLicenseNumber)
            .HasMaxLength(50);

        builder.Property(x => x.MohreLicenseNumber)
            .HasMaxLength(50);

        builder.Property(x => x.TradeLicenseNumber)
            .HasMaxLength(50);

        builder.Property(x => x.Emirate)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.TaxRegistrationNumber)
            .HasMaxLength(20);

        // Unique constraint on Tadbeer license number
        builder.HasIndex(x => x.TadbeerLicenseNumber)
            .IsUnique()
            .HasDatabaseName("ix_tenants_tadbeer_license")
            .HasFilter("tadbeer_license_number IS NOT NULL");

        // Index for emirate queries
        builder.HasIndex(x => x.Emirate)
            .HasDatabaseName("ix_tenants_emirate");

        // Index for active agencies
        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("ix_tenants_is_active");

        // Relationship to licenses
        builder.HasMany(x => x.Licenses)
            .WithOne(x => x.Tenant)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship to outgoing pool agreements (this tenant is providing workers)
        builder.HasMany(x => x.OutgoingPoolAgreements)
            .WithOne(x => x.FromTenant)
            .HasForeignKey(x => x.FromTenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship to incoming pool agreements (this tenant is receiving workers)
        builder.HasMany(x => x.IncomingPoolAgreements)
            .WithOne(x => x.ToTenant)
            .HasForeignKey(x => x.ToTenantId)
            .OnDelete(DeleteBehavior.Restrict);

        #endregion

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
    }
}
