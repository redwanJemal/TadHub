using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Worker.Core.Entities;

namespace Worker.Core.Persistence;

public class WorkerLanguageConfiguration : IEntityTypeConfiguration<WorkerLanguage>
{
    public void Configure(EntityTypeBuilder<WorkerLanguage> builder)
    {
        builder.ToTable("worker_languages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Language)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ProficiencyLevel)
            .IsRequired()
            .HasMaxLength(30);

        builder.HasIndex(x => new { x.WorkerId, x.Language })
            .IsUnique()
            .HasDatabaseName("ix_worker_languages_worker_id_language");

        builder.HasIndex(x => x.WorkerId)
            .HasDatabaseName("ix_worker_languages_worker_id");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_worker_languages_tenant_id");
    }
}
