using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Worker.Core.Entities;

namespace Worker.Core.Persistence;

public class WorkerStatusHistoryConfiguration : IEntityTypeConfiguration<WorkerStatusHistory>
{
    public void Configure(EntityTypeBuilder<WorkerStatusHistory> builder)
    {
        builder.ToTable("worker_status_history");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FromStatus)
            .HasMaxLength(30)
            .HasConversion<string?>();

        builder.Property(x => x.ToStatus)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.Reason)
            .HasMaxLength(500);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(x => new { x.WorkerId, x.ChangedAt })
            .HasDatabaseName("ix_worker_status_history_worker_id_changed_at");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_worker_status_history_tenant_id");
    }
}
