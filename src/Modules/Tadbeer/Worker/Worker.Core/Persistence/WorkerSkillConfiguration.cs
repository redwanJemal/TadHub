using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Worker.Core.Entities;

namespace Worker.Core.Persistence;

/// <summary>
/// EF Core configuration for WorkerSkill entity.
/// </summary>
public class WorkerSkillConfiguration : IEntityTypeConfiguration<WorkerSkill>
{
    public void Configure(EntityTypeBuilder<WorkerSkill> builder)
    {
        builder.ToTable("worker_skills");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.WorkerId)
            .IsRequired();

        builder.Property(x => x.SkillName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Rating)
            .IsRequired();

        // Composite unique index (worker + skill)
        builder.HasIndex(x => new { x.WorkerId, x.SkillName })
            .IsUnique()
            .HasDatabaseName("ix_worker_skills_worker_skill");

        // Check constraint for rating range
        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "ck_worker_skills_rating",
                "rating >= 0 AND rating <= 100");
        });
    }
}
