using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Supplier.Core.Persistence;

/// <summary>
/// EF Core configuration for Supplier entity.
/// </summary>
public class SupplierConfiguration : IEntityTypeConfiguration<Entities.Supplier>
{
    public void Configure(EntityTypeBuilder<Entities.Supplier> builder)
    {
        builder.ToTable("suppliers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.NameEn)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.NameAr)
            .HasMaxLength(255);

        builder.Property(x => x.Country)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.City)
            .HasMaxLength(100);

        builder.Property(x => x.LicenseNumber)
            .HasMaxLength(100);

        builder.Property(x => x.Phone)
            .HasMaxLength(50);

        builder.Property(x => x.Email)
            .HasMaxLength(255);

        builder.Property(x => x.Website)
            .HasMaxLength(500);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        // Unique index on LicenseNumber where not null
        builder.HasIndex(x => x.LicenseNumber)
            .IsUnique()
            .HasFilter("license_number IS NOT NULL")
            .HasDatabaseName("ix_suppliers_license_number");

        // Index for common queries
        builder.HasIndex(x => x.Country)
            .HasDatabaseName("ix_suppliers_country");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("ix_suppliers_is_active");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("ix_suppliers_created_at");

        builder.HasIndex(x => x.NameEn)
            .HasDatabaseName("ix_suppliers_name_en");
    }
}
