using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Worker.Core.Entities;

namespace Worker.Core.Persistence;

/// <summary>
/// EF Core configuration for WorkerMedia entity.
/// </summary>
public class WorkerMediaConfiguration : IEntityTypeConfiguration<WorkerMedia>
{
    public void Configure(EntityTypeBuilder<WorkerMedia> builder)
    {
        builder.ToTable("worker_media");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.WorkerId)
            .IsRequired();

        builder.Property(x => x.MediaType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.FileUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.UploadedAt)
            .IsRequired();

        // Index for worker media queries
        builder.HasIndex(x => new { x.WorkerId, x.MediaType })
            .HasDatabaseName("ix_worker_media_worker_type");

        // Index for primary media
        builder.HasIndex(x => new { x.WorkerId, x.IsPrimary })
            .HasDatabaseName("ix_worker_media_worker_primary")
            .HasFilter("is_primary = true");
    }
}
