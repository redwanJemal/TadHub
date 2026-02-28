using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Financial.Core.Entities;

namespace Financial.Core.Persistence;

public class SupplierPaymentConfiguration : IEntityTypeConfiguration<SupplierPayment>
{
    public void Configure(EntityTypeBuilder<SupplierPayment> builder)
    {
        builder.ToTable("supplier_payments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PaymentNumber)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.Method)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.Amount).HasPrecision(18, 2);

        builder.Property(x => x.ReferenceNumber).HasMaxLength(100);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        // Indexes
        builder.HasIndex(x => new { x.TenantId, x.PaymentNumber })
            .IsUnique()
            .HasDatabaseName("ix_supplier_payments_tenant_id_payment_number");

        builder.HasIndex(x => new { x.TenantId, x.Status })
            .HasDatabaseName("ix_supplier_payments_tenant_id_status");

        builder.HasIndex(x => new { x.TenantId, x.SupplierId })
            .HasDatabaseName("ix_supplier_payments_tenant_id_supplier_id");

        builder.HasIndex(x => new { x.TenantId, x.WorkerId })
            .HasDatabaseName("ix_supplier_payments_tenant_id_worker_id");

        builder.HasIndex(x => new { x.TenantId, x.ContractId })
            .HasDatabaseName("ix_supplier_payments_tenant_id_contract_id");

        builder.HasIndex(x => new { x.TenantId, x.PaymentDate })
            .HasDatabaseName("ix_supplier_payments_tenant_id_payment_date");
    }
}
