using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trial.Core.Entities;

namespace Trial.Core.Persistence;

public class TrialStatusHistoryConfiguration : IEntityTypeConfiguration<TrialStatusHistory>
{
    public void Configure(EntityTypeBuilder<TrialStatusHistory> builder)
    {
        builder.ToTable("trial_status_history");

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
        builder.HasIndex(x => new { x.TrialId, x.ChangedAt })
            .HasDatabaseName("ix_trial_history_trial");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_trial_history_tenant_id");
    }
}
