using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenancy.Core.Entities;

namespace Tenancy.Core.Persistence;

/// <summary>
/// EF Core configuration for SharedPoolWorker entity.
/// </summary>
public class SharedPoolWorkerConfiguration : IEntityTypeConfiguration<SharedPoolWorker>
{
    public void Configure(EntityTypeBuilder<SharedPoolWorker> builder)
    {
        builder.ToTable("shared_pool_workers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SharedPoolAgreementId)
            .IsRequired();

        builder.Property(x => x.WorkerId)
            .IsRequired();

        builder.Property(x => x.SharedAt)
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        // Relationship to agreement
        builder.HasOne(x => x.SharedPoolAgreement)
            .WithMany(x => x.SharedWorkers)
            .HasForeignKey(x => x.SharedPoolAgreementId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for agreement queries
        builder.HasIndex(x => x.SharedPoolAgreementId)
            .HasDatabaseName("ix_shared_pool_workers_agreement");

        // Index for worker queries
        builder.HasIndex(x => x.WorkerId)
            .HasDatabaseName("ix_shared_pool_workers_worker");

        // Index for active workers in pool
        builder.HasIndex(x => new { x.SharedPoolAgreementId, x.RevokedAt })
            .HasDatabaseName("ix_shared_pool_workers_agreement_active")
            .HasFilter("revoked_at IS NULL");

        // Unique constraint: worker can only be in one active share per agreement
        builder.HasIndex(x => new { x.SharedPoolAgreementId, x.WorkerId })
            .HasDatabaseName("ix_shared_pool_workers_agreement_worker")
            .HasFilter("revoked_at IS NULL")
            .IsUnique();
    }
}
