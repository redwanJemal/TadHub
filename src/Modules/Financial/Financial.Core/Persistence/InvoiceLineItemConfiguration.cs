using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Financial.Core.Entities;

namespace Financial.Core.Persistence;

public class InvoiceLineItemConfiguration : IEntityTypeConfiguration<InvoiceLineItem>
{
    public void Configure(EntityTypeBuilder<InvoiceLineItem> builder)
    {
        builder.ToTable("invoice_line_items");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.DescriptionAr)
            .HasMaxLength(500);

        builder.Property(x => x.ItemCode)
            .HasMaxLength(50);

        builder.Property(x => x.Quantity).HasPrecision(18, 2);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.DiscountAmount).HasPrecision(18, 2);
        builder.Property(x => x.LineTotal).HasPrecision(18, 2);

        // Indexes
        builder.HasIndex(x => new { x.TenantId, x.InvoiceId })
            .HasDatabaseName("ix_invoice_line_items_tenant_id_invoice_id");
    }
}
