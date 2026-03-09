using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Placement.Core.Entities;

namespace Placement.Core.Persistence;

public class PlacementStatusHistoryConfiguration : IEntityTypeConfiguration<PlacementStatusHistory>
{
    public void Configure(EntityTypeBuilder<PlacementStatusHistory> builder)
    {
        builder.ToTable("placement_status_history");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FromStatus)
            .HasMaxLength(30)
            .HasConversion<string?>();

        builder.Property(x => x.ToStatus)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.ChangedBy)
            .HasMaxLength(200);

        builder.Property(x => x.Reason)
            .HasMaxLength(500);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        // Indexes
        builder.HasIndex(x => new { x.PlacementId, x.ChangedAt })
            .HasDatabaseName("ix_placement_history_placement");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_placement_history_tenant_id");
    }
}
