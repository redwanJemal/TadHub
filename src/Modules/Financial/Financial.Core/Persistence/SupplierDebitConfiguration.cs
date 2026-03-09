using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Financial.Core.Entities;

namespace Financial.Core.Persistence;

public class SupplierDebitConfiguration : IEntityTypeConfiguration<SupplierDebit>
{
    public void Configure(EntityTypeBuilder<SupplierDebit> builder)
    {
        builder.ToTable("supplier_debits");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DebitNumber)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.DebitType)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.CaseType)
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.Amount).HasPrecision(18, 2);

        builder.Property(x => x.Notes).HasMaxLength(2000);

        // Indexes
        builder.HasIndex(x => new { x.TenantId, x.DebitNumber })
            .IsUnique()
            .HasDatabaseName("ix_supplier_debits_tenant_id_debit_number");

        builder.HasIndex(x => new { x.TenantId, x.Status })
            .HasDatabaseName("ix_supplier_debits_tenant_id_status");

        builder.HasIndex(x => new { x.TenantId, x.SupplierId })
            .HasDatabaseName("ix_supplier_debits_tenant_id_supplier_id");

        builder.HasIndex(x => new { x.TenantId, x.WorkerId })
            .HasDatabaseName("ix_supplier_debits_tenant_id_worker_id");

        builder.HasIndex(x => new { x.TenantId, x.ContractId })
            .HasDatabaseName("ix_supplier_debits_tenant_id_contract_id");

        builder.HasIndex(x => new { x.TenantId, x.CaseType, x.CaseId })
            .HasDatabaseName("ix_supplier_debits_tenant_id_case_type_case_id");
    }
}
