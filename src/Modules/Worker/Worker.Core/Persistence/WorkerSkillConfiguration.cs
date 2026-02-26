using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Worker.Core.Entities;

namespace Worker.Core.Persistence;

public class WorkerSkillConfiguration : IEntityTypeConfiguration<WorkerSkill>
{
    public void Configure(EntityTypeBuilder<WorkerSkill> builder)
    {
        builder.ToTable("worker_skills");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SkillName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ProficiencyLevel)
            .IsRequired()
            .HasMaxLength(30);

        builder.HasIndex(x => new { x.WorkerId, x.SkillName })
            .IsUnique()
            .HasDatabaseName("ix_worker_skills_worker_id_skill_name");

        builder.HasIndex(x => x.WorkerId)
            .HasDatabaseName("ix_worker_skills_worker_id");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_worker_skills_tenant_id");
    }
}
