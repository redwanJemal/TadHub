using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Candidate.Core.Entities;

namespace Candidate.Core.Persistence;

/// <summary>
/// EF Core configuration for CandidateLanguage entity.
/// </summary>
public class CandidateLanguageConfiguration : IEntityTypeConfiguration<CandidateLanguage>
{
    public void Configure(EntityTypeBuilder<CandidateLanguage> builder)
    {
        builder.ToTable("candidate_languages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Language)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ProficiencyLevel)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        // Unique constraint: one language per candidate
        builder.HasIndex(x => new { x.CandidateId, x.Language })
            .IsUnique()
            .HasDatabaseName("ix_candidate_languages_candidate_id_language");

        builder.HasIndex(x => x.CandidateId)
            .HasDatabaseName("ix_candidate_languages_candidate_id");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_candidate_languages_tenant_id");
    }
}
