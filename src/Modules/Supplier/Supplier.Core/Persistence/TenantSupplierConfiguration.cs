using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Supplier.Core.Entities;

namespace Supplier.Core.Persistence;

/// <summary>
/// EF Core configuration for TenantSupplier entity.
/// </summary>
public class TenantSupplierConfiguration : IEntityTypeConfiguration<TenantSupplier>
{
    public void Configure(EntityTypeBuilder<TenantSupplier> builder)
    {
        builder.ToTable("tenant_suppliers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(x => x.ContractReference)
            .HasMaxLength(100);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        // Unique index on (TenantId, SupplierId) — each tenant can link to a supplier only once
        builder.HasIndex(x => new { x.TenantId, x.SupplierId })
            .IsUnique()
            .HasDatabaseName("ix_tenant_suppliers_tenant_id_supplier_id");

        // FK to Supplier with Restrict delete (don't cascade tenant-supplier deletion to global supplier)
        builder.HasOne(x => x.Supplier)
            .WithMany(x => x.TenantRelationships)
            .HasForeignKey(x => x.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for common queries
        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_tenant_suppliers_tenant_id");

        builder.HasIndex(x => x.SupplierId)
            .HasDatabaseName("ix_tenant_suppliers_supplier_id");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("ix_tenant_suppliers_status");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("ix_tenant_suppliers_created_at");
    }
}
