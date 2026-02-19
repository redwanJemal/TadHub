using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TadHub.SharedKernel.Localization;
using Worker.Core.Entities;

namespace Worker.Core.Persistence;

/// <summary>
/// EF Core configuration for JobCategory entity.
/// Global entity (not tenant-scoped).
/// </summary>
public class JobCategoryConfiguration : IEntityTypeConfiguration<JobCategory>
{
    public void Configure(EntityTypeBuilder<JobCategory> builder)
    {
        builder.ToTable("job_categories");

        builder.HasKey(x => x.Id);

        // Localized name stored as JSON
        builder.Property(x => x.Name)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<LocalizedString>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new LocalizedString())
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.MoHRECode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // Unique MoHRE code
        builder.HasIndex(x => x.MoHRECode)
            .IsUnique()
            .HasDatabaseName("ix_job_categories_mohre_code");

        // Index for active categories ordered by display
        builder.HasIndex(x => new { x.IsActive, x.DisplayOrder })
            .HasDatabaseName("ix_job_categories_active_order");
    }
}
