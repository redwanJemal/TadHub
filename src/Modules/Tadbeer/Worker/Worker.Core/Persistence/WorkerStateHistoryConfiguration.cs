using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Worker.Core.Entities;

namespace Worker.Core.Persistence;

/// <summary>
/// EF Core configuration for WorkerStateHistory entity.
/// Append-only audit trail.
/// </summary>
public class WorkerStateHistoryConfiguration : IEntityTypeConfiguration<WorkerStateHistory>
{
    public void Configure(EntityTypeBuilder<WorkerStateHistory> builder)
    {
        builder.ToTable("worker_state_history");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.WorkerId)
            .IsRequired();

        builder.Property(x => x.FromStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.ToStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Reason)
            .HasMaxLength(500);

        builder.Property(x => x.TriggeredByUserId)
            .IsRequired();

        builder.Property(x => x.OccurredAt)
            .IsRequired();

        // Index for worker state history (ordered by occurred)
        builder.HasIndex(x => new { x.WorkerId, x.OccurredAt })
            .HasDatabaseName("ix_worker_state_history_worker_occurred");

        // Index for finding transitions to a specific state
        builder.HasIndex(x => x.ToStatus)
            .HasDatabaseName("ix_worker_state_history_to_status");

        // Index for related entity lookups
        builder.HasIndex(x => x.RelatedEntityId)
            .HasDatabaseName("ix_worker_state_history_related_entity")
            .HasFilter("related_entity_id IS NOT NULL");
    }
}
