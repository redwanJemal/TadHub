using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenancy.Core.Entities;

namespace Tenancy.Core.Persistence;

/// <summary>
/// EF Core configuration for TadbeerLicense entity.
/// </summary>
public class TadbeerLicenseConfiguration : IEntityTypeConfiguration<TadbeerLicense>
{
    public void Configure(EntityTypeBuilder<TadbeerLicense> builder)
    {
        builder.ToTable("tadbeer_licenses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.LicenseType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Number)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.IssuedAt)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.DocumentUrl)
            .HasMaxLength(500);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        // Index for tenant + type combination
        builder.HasIndex(x => new { x.TenantId, x.LicenseType })
            .HasDatabaseName("ix_tadbeer_licenses_tenant_type");

        // Index for expiry date (for daily job queries)
        builder.HasIndex(x => x.ExpiresAt)
            .HasDatabaseName("ix_tadbeer_licenses_expires_at");

        // Index for status
        builder.HasIndex(x => x.Status)
            .HasDatabaseName("ix_tadbeer_licenses_status");

        // Unique constraint on license number within type
        builder.HasIndex(x => new { x.LicenseType, x.Number })
            .IsUnique()
            .HasDatabaseName("ix_tadbeer_licenses_type_number");
    }
}
