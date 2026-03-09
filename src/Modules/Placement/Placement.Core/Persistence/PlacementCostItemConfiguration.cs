using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Placement.Core.Entities;

namespace Placement.Core.Persistence;

public class PlacementCostItemConfiguration : IEntityTypeConfiguration<PlacementCostItem>
{
    public void Configure(EntityTypeBuilder<PlacementCostItem> builder)
    {
        builder.ToTable("placement_cost_items");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CostType)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(x => x.ReferenceNumber)
            .HasMaxLength(100);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        // Indexes
        builder.HasIndex(x => x.PlacementId)
            .HasDatabaseName("ix_placement_costs_placement");

        builder.HasIndex(x => new { x.TenantId, x.Status })
            .HasDatabaseName("ix_placement_costs_tenant_status");
    }
}
