using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Worker.Core.Entities;

namespace Worker.Core.Persistence;

/// <summary>
/// EF Core configuration for Worker entity.
/// </summary>
public class WorkerConfiguration : IEntityTypeConfiguration<Entities.Worker>
{
    public void Configure(EntityTypeBuilder<Entities.Worker> builder)
    {
        builder.ToTable("workers");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PassportNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.EmiratesId)
            .HasMaxLength(20);

        builder.Property(x => x.CvSerial)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.FullNameEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.FullNameAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Nationality)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Gender)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.Religion)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.MaritalStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Education)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.CurrentStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.PassportLocation)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.MonthlyBaseSalary)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(x => x.PhotoUrl)
            .HasMaxLength(500);

        builder.Property(x => x.VideoUrl)
            .HasMaxLength(500);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        // Unique passport within tenant
        builder.HasIndex(x => new { x.TenantId, x.PassportNumber })
            .IsUnique()
            .HasDatabaseName("ix_workers_tenant_passport");

        // Unique CV serial within tenant
        builder.HasIndex(x => new { x.TenantId, x.CvSerial })
            .IsUnique()
            .HasDatabaseName("ix_workers_tenant_cv_serial");

        // Index for status filtering (array filter support)
        builder.HasIndex(x => x.CurrentStatus)
            .HasDatabaseName("ix_workers_current_status");

        // Index for nationality filtering (array filter support)
        builder.HasIndex(x => x.Nationality)
            .HasDatabaseName("ix_workers_nationality");

        // Index for job category
        builder.HasIndex(x => x.JobCategoryId)
            .HasDatabaseName("ix_workers_job_category_id");

        // Index for passport location
        builder.HasIndex(x => x.PassportLocation)
            .HasDatabaseName("ix_workers_passport_location");

        // Index for flexible availability
        builder.HasIndex(x => x.IsAvailableForFlexible)
            .HasDatabaseName("ix_workers_is_available_flexible");

        // Index for created date
        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("ix_workers_created_at");

        // Full-text search indexes
        builder.HasIndex(x => x.FullNameEn)
            .HasDatabaseName("ix_workers_full_name_en");

        builder.HasIndex(x => x.FullNameAr)
            .HasDatabaseName("ix_workers_full_name_ar");

        // Relationships
        builder.HasOne(x => x.JobCategory)
            .WithMany() // No inverse navigation in JobCategory
            .HasForeignKey(x => x.JobCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Skills)
            .WithOne(x => x.Worker)
            .HasForeignKey(x => x.WorkerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Languages)
            .WithOne(x => x.Worker)
            .HasForeignKey(x => x.WorkerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Media)
            .WithOne(x => x.Worker)
            .HasForeignKey(x => x.WorkerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.PassportCustodyHistory)
            .WithOne(x => x.Worker)
            .HasForeignKey(x => x.WorkerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.StateHistory)
            .WithOne(x => x.Worker)
            .HasForeignKey(x => x.WorkerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
