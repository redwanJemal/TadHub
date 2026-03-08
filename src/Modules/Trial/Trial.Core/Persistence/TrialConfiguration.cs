using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Trial.Core.Entities;

namespace Trial.Core.Persistence;

public class TrialConfiguration : IEntityTypeConfiguration<Entities.Trial>
{
    public void Configure(EntityTypeBuilder<Entities.Trial> builder)
    {
        builder.ToTable("trials");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TrialCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.Outcome)
            .HasMaxLength(30)
            .HasConversion<string?>();

        builder.Property(x => x.OutcomeNotes)
            .HasMaxLength(2000);

        // Relationships
        builder.HasMany(x => x.StatusHistory)
            .WithOne(x => x.Trial)
            .HasForeignKey(x => x.TrialId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.TenantId, x.TrialCode })
            .IsUnique()
            .HasDatabaseName("ix_trials_tenant_code");

        builder.HasIndex(x => new { x.TenantId, x.Status })
            .HasDatabaseName("ix_trials_tenant_status");

        builder.HasIndex(x => new { x.TenantId, x.WorkerId })
            .HasDatabaseName("ix_trials_tenant_worker");

        builder.HasIndex(x => new { x.TenantId, x.ClientId })
            .HasDatabaseName("ix_trials_tenant_client");

        builder.HasIndex(x => new { x.TenantId, x.PlacementId })
            .HasDatabaseName("ix_trials_tenant_placement");
    }
}
