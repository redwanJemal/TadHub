using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Financial.Core.Entities;

namespace Financial.Core.Persistence;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.MilestoneType)
            .HasMaxLength(30)
            .HasConversion<string?>();

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.TenantTrn).HasMaxLength(50);
        builder.Property(x => x.ClientTrn).HasMaxLength(50);

        // Financial amounts
        builder.Property(x => x.Subtotal).HasPrecision(18, 2);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        builder.Property(x => x.TaxableAmount).HasPrecision(18, 2);
        builder.Property(x => x.VatRate).HasPrecision(18, 2);
        builder.Property(x => x.VatAmount).HasPrecision(18, 2);
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.PaidAmount).HasPrecision(18, 2);
        builder.Property(x => x.BalanceDue).HasPrecision(18, 2);
        builder.Property(x => x.DiscountPercentage).HasPrecision(18, 2);

        // Discount
        builder.Property(x => x.DiscountProgramName).HasMaxLength(200);
        builder.Property(x => x.DiscountCardNumber).HasMaxLength(100);

        // Credit note
        builder.Property(x => x.CreditNoteReason).HasMaxLength(500);

        builder.Property(x => x.Notes).HasMaxLength(2000);

        // Relationships
        builder.HasMany(x => x.LineItems)
            .WithOne(x => x.Invoice)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Payments)
            .WithOne(x => x.Invoice)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.TenantId, x.InvoiceNumber })
            .IsUnique()
            .HasDatabaseName("ix_invoices_tenant_id_invoice_number");

        builder.HasIndex(x => new { x.TenantId, x.Status })
            .HasDatabaseName("ix_invoices_tenant_id_status");

        builder.HasIndex(x => new { x.TenantId, x.ClientId })
            .HasDatabaseName("ix_invoices_tenant_id_client_id");

        builder.HasIndex(x => new { x.TenantId, x.ContractId })
            .HasDatabaseName("ix_invoices_tenant_id_contract_id");

        builder.HasIndex(x => new { x.TenantId, x.IssueDate })
            .HasDatabaseName("ix_invoices_tenant_id_issue_date");

        builder.HasIndex(x => new { x.TenantId, x.DueDate })
            .HasDatabaseName("ix_invoices_tenant_id_due_date");
    }
}
