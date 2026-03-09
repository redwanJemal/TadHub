using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReferenceData.Core.Entities;

namespace ReferenceData.Core.Persistence;

/// <summary>
/// EF Core configuration for CountryPackage entity.
/// Tenant-scoped with soft delete support.
/// </summary>
public class CountryPackageConfiguration : IEntityTypeConfiguration<CountryPackage>
{
    public void Configure(EntityTypeBuilder<CountryPackage> builder)
    {
        builder.ToTable("country_packages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        // Cost fields
        builder.Property(x => x.MaidCost).HasPrecision(18, 2);
        builder.Property(x => x.MonthlyAccommodationCost).HasPrecision(18, 2);
        builder.Property(x => x.VisaCost).HasPrecision(18, 2);
        builder.Property(x => x.EmploymentVisaCost).HasPrecision(18, 2);
        builder.Property(x => x.ResidenceVisaCost).HasPrecision(18, 2);
        builder.Property(x => x.MedicalCost).HasPrecision(18, 2);
        builder.Property(x => x.TransportationCost).HasPrecision(18, 2);
        builder.Property(x => x.TicketCost).HasPrecision(18, 2);
        builder.Property(x => x.InsuranceCost).HasPrecision(18, 2);
        builder.Property(x => x.EmiratesIdCost).HasPrecision(18, 2);
        builder.Property(x => x.OtherCosts).HasPrecision(18, 2);
        builder.Property(x => x.TotalPackagePrice).HasPrecision(18, 2);

        // Commission
        builder.Property(x => x.SupplierCommission).HasPrecision(18, 2);
        builder.Property(x => x.SupplierCommissionType)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        // Guarantee period
        builder.Property(x => x.DefaultGuaranteePeriod)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.EffectiveFrom)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        // Indexes
        builder.HasIndex(x => new { x.TenantId, x.CountryId })
            .HasDatabaseName("ix_country_packages_tenant_country");

        builder.HasIndex(x => new { x.TenantId, x.IsActive })
            .HasDatabaseName("ix_country_packages_tenant_active");

        builder.HasIndex(x => new { x.TenantId, x.CountryId, x.IsDefault })
            .HasFilter("is_default = true AND is_deleted = false")
            .HasDatabaseName("ix_country_packages_tenant_country_default");
    }
}
