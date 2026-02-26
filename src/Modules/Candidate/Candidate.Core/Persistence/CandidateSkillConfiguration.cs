using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Candidate.Core.Entities;

namespace Candidate.Core.Persistence;

/// <summary>
/// EF Core configuration for CandidateSkill entity.
/// </summary>
public class CandidateSkillConfiguration : IEntityTypeConfiguration<CandidateSkill>
{
    public void Configure(EntityTypeBuilder<CandidateSkill> builder)
    {
        builder.ToTable("candidate_skills");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SkillName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ProficiencyLevel)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        // Unique constraint: one skill name per candidate
        builder.HasIndex(x => new { x.CandidateId, x.SkillName })
            .IsUnique()
            .HasDatabaseName("ix_candidate_skills_candidate_id_skill_name");

        builder.HasIndex(x => x.CandidateId)
            .HasDatabaseName("ix_candidate_skills_candidate_id");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_candidate_skills_tenant_id");
    }
}
