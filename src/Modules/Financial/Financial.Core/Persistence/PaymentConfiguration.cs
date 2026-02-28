using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Financial.Core.Entities;

namespace Financial.Core.Persistence;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

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
        builder.Property(x => x.RefundAmount).HasPrecision(18, 2);

        builder.Property(x => x.ReferenceNumber).HasMaxLength(100);
        builder.Property(x => x.GatewayProvider).HasMaxLength(50);
        builder.Property(x => x.GatewayTransactionId).HasMaxLength(200);
        builder.Property(x => x.GatewayStatus).HasMaxLength(50);
        builder.Property(x => x.CashierName).HasMaxLength(200);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        // Indexes
        builder.HasIndex(x => new { x.TenantId, x.PaymentNumber })
            .IsUnique()
            .HasDatabaseName("ix_payments_tenant_id_payment_number");

        builder.HasIndex(x => new { x.TenantId, x.Status })
            .HasDatabaseName("ix_payments_tenant_id_status");

        builder.HasIndex(x => new { x.TenantId, x.InvoiceId })
            .HasDatabaseName("ix_payments_tenant_id_invoice_id");

        builder.HasIndex(x => new { x.TenantId, x.ClientId })
            .HasDatabaseName("ix_payments_tenant_id_client_id");

        builder.HasIndex(x => new { x.TenantId, x.PaymentDate })
            .HasDatabaseName("ix_payments_tenant_id_payment_date");
    }
}
