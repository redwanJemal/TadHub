using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Runaway.Core.Entities;

namespace Runaway.Core.Persistence;

public class RunawayCaseStatusHistoryConfiguration : IEntityTypeConfiguration<RunawayCaseStatusHistory>
{
    public void Configure(EntityTypeBuilder<RunawayCaseStatusHistory> builder)
    {
        builder.ToTable("runaway_case_status_history");

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
        builder.HasIndex(x => new { x.RunawayCaseId, x.ChangedAt })
            .HasDatabaseName("ix_runaway_history_case");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_runaway_history_tenant");
    }
}
