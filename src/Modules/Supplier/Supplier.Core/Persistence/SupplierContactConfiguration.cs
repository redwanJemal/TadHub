using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Supplier.Core.Entities;

namespace Supplier.Core.Persistence;

/// <summary>
/// EF Core configuration for SupplierContact entity.
/// </summary>
public class SupplierContactConfiguration : IEntityTypeConfiguration<SupplierContact>
{
    public void Configure(EntityTypeBuilder<SupplierContact> builder)
    {
        builder.ToTable("supplier_contacts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Email)
            .HasMaxLength(255);

        builder.Property(x => x.Phone)
            .HasMaxLength(50);

        builder.Property(x => x.JobTitle)
            .HasMaxLength(100);

        builder.Property(x => x.IsPrimary)
            .HasDefaultValue(false);

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        // Unique index on (SupplierId, Email) where email is not null
        builder.HasIndex(x => new { x.SupplierId, x.Email })
            .IsUnique()
            .HasFilter("email IS NOT NULL")
            .HasDatabaseName("ix_supplier_contacts_supplier_id_email");

        // FK to Supplier with Cascade delete
        builder.HasOne(x => x.Supplier)
            .WithMany(x => x.Contacts)
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for common queries
        builder.HasIndex(x => x.SupplierId)
            .HasDatabaseName("ix_supplier_contacts_supplier_id");

        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("ix_supplier_contacts_is_active");
    }
}
