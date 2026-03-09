using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Arrival.Core.Entities;

namespace Arrival.Core.Persistence;

public class ArrivalStatusHistoryConfiguration : IEntityTypeConfiguration<ArrivalStatusHistory>
{
    public void Configure(EntityTypeBuilder<ArrivalStatusHistory> builder)
    {
        builder.ToTable("arrival_status_history");

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
        builder.HasIndex(x => new { x.ArrivalId, x.ChangedAt })
            .HasDatabaseName("ix_arrival_history_arrival");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_arrival_history_tenant_id");
    }
}
