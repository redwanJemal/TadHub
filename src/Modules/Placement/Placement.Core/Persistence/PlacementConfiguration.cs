using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Placement.Core.Entities;

namespace Placement.Core.Persistence;

public class PlacementConfiguration : IEntityTypeConfiguration<Entities.Placement>
{
    public void Configure(EntityTypeBuilder<Entities.Placement> builder)
    {
        builder.ToTable("placements");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PlacementCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.FlowType)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(x => x.StatusReason)
            .HasMaxLength(500);

        builder.Property(x => x.BookedByName)
            .HasMaxLength(200);

        builder.Property(x => x.BookingNotes)
            .HasMaxLength(2000);

        builder.Property(x => x.FlightDetails)
            .HasMaxLength(500);

        builder.Property(x => x.CancellationReason)
            .HasMaxLength(500);

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(10);

        // Relationships
        builder.HasMany(x => x.CostItems)
            .WithOne(x => x.Placement)
            .HasForeignKey(x => x.PlacementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.StatusHistory)
            .WithOne(x => x.Placement)
            .HasForeignKey(x => x.PlacementId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.TenantId, x.PlacementCode })
            .IsUnique()
            .HasDatabaseName("ix_placements_tenant_code");

        builder.HasIndex(x => new { x.TenantId, x.Status })
            .HasDatabaseName("ix_placements_tenant_status");

        builder.HasIndex(x => new { x.TenantId, x.CandidateId })
            .HasDatabaseName("ix_placements_tenant_candidate");

        builder.HasIndex(x => new { x.TenantId, x.ClientId })
            .HasDatabaseName("ix_placements_tenant_client");

        builder.HasIndex(x => new { x.TenantId, x.WorkerId })
            .HasDatabaseName("ix_placements_tenant_worker");
    }
}
