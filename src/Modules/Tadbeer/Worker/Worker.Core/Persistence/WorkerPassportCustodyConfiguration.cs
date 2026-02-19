using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Worker.Core.Entities;

namespace Worker.Core.Persistence;

/// <summary>
/// EF Core configuration for WorkerPassportCustody entity.
/// Append-only audit trail.
/// </summary>
public class WorkerPassportCustodyConfiguration : IEntityTypeConfiguration<WorkerPassportCustody>
{
    public void Configure(EntityTypeBuilder<WorkerPassportCustody> builder)
    {
        builder.ToTable("worker_passport_custody");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.WorkerId)
            .IsRequired();

        builder.Property(x => x.Location)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.HandedToName)
            .HasMaxLength(200);

        builder.Property(x => x.RecordedByUserId)
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        // Index for worker custody history (ordered by created)
        builder.HasIndex(x => new { x.WorkerId, x.CreatedAt })
            .HasDatabaseName("ix_worker_passport_custody_worker_created");

        // Index for finding by handed-to entity
        builder.HasIndex(x => x.HandedToEntityId)
            .HasDatabaseName("ix_worker_passport_custody_handed_to_entity")
            .HasFilter("handed_to_entity_id IS NOT NULL");
    }
}
