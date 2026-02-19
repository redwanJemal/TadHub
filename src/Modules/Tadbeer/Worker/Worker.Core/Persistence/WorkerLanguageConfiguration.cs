using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Worker.Core.Entities;

namespace Worker.Core.Persistence;

/// <summary>
/// EF Core configuration for WorkerLanguage entity.
/// </summary>
public class WorkerLanguageConfiguration : IEntityTypeConfiguration<WorkerLanguage>
{
    public void Configure(EntityTypeBuilder<WorkerLanguage> builder)
    {
        builder.ToTable("worker_languages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.WorkerId)
            .IsRequired();

        builder.Property(x => x.Language)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Proficiency)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Composite unique index (worker + language)
        builder.HasIndex(x => new { x.WorkerId, x.Language })
            .IsUnique()
            .HasDatabaseName("ix_worker_languages_worker_language");
    }
}
