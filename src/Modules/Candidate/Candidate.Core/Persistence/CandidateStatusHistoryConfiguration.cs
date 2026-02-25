using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Candidate.Core.Entities;

namespace Candidate.Core.Persistence;

/// <summary>
/// EF Core configuration for CandidateStatusHistory entity.
/// </summary>
public class CandidateStatusHistoryConfiguration : IEntityTypeConfiguration<CandidateStatusHistory>
{
    public void Configure(EntityTypeBuilder<CandidateStatusHistory> builder)
    {
        builder.ToTable("candidate_status_history");

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

        // Indexes
        builder.HasIndex(x => x.CandidateId)
            .HasDatabaseName("ix_candidate_status_history_candidate_id");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_candidate_status_history_tenant_id");

        builder.HasIndex(x => x.ChangedAt)
            .HasDatabaseName("ix_candidate_status_history_changed_at");
    }
}
