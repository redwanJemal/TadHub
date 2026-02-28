using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Financial.Core.Entities;

namespace Financial.Core.Persistence;

public class CashReconciliationConfiguration : IEntityTypeConfiguration<CashReconciliation>
{
    public void Configure(EntityTypeBuilder<CashReconciliation> builder)
    {
        builder.ToTable("cash_reconciliations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CashierName).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.Property(x => x.CashTotal).HasPrecision(18, 2);
        builder.Property(x => x.CardTotal).HasPrecision(18, 2);
        builder.Property(x => x.BankTransferTotal).HasPrecision(18, 2);
        builder.Property(x => x.ChequeTotal).HasPrecision(18, 2);
        builder.Property(x => x.EDirhamTotal).HasPrecision(18, 2);
        builder.Property(x => x.OnlineTotal).HasPrecision(18, 2);
        builder.Property(x => x.GrandTotal).HasPrecision(18, 2);

        // Indexes
        builder.HasIndex(x => new { x.TenantId, x.ReportDate })
            .HasDatabaseName("ix_cash_reconciliations_tenant_id_report_date");

        builder.HasIndex(x => new { x.TenantId, x.CashierId })
            .HasDatabaseName("ix_cash_reconciliations_tenant_id_cashier_id");
    }
}
