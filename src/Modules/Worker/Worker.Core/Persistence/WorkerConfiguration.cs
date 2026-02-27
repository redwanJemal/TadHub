using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Worker.Core.Entities;

namespace Worker.Core.Persistence;

public class WorkerConfiguration : IEntityTypeConfiguration<Entities.Worker>
{
    public void Configure(EntityTypeBuilder<Entities.Worker> builder)
    {
        builder.ToTable("workers");

        builder.HasKey(x => x.Id);

        // Worker-specific
        builder.Property(x => x.WorkerCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.Location)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(x => x.StatusReason)
            .HasMaxLength(500);

        builder.Property(x => x.TerminationReason)
            .HasMaxLength(500);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        // Source
        builder.Property(x => x.SourceType)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        // Personal
        builder.Property(x => x.FullNameEn)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.FullNameAr)
            .HasMaxLength(255);

        builder.Property(x => x.Nationality)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.Gender)
            .HasMaxLength(20);

        builder.Property(x => x.PassportNumber)
            .HasMaxLength(50);

        builder.Property(x => x.Phone)
            .HasMaxLength(50);

        builder.Property(x => x.Email)
            .HasMaxLength(255);

        // Professional
        builder.Property(x => x.Religion)
            .HasMaxLength(50);

        builder.Property(x => x.MaritalStatus)
            .HasMaxLength(20);

        builder.Property(x => x.EducationLevel)
            .HasMaxLength(50);

        builder.Property(x => x.MonthlySalary)
            .HasPrecision(18, 2);

        // Media
        builder.Property(x => x.PhotoUrl)
            .HasMaxLength(500);

        builder.Property(x => x.VideoUrl)
            .HasMaxLength(500);

        builder.Property(x => x.PassportDocumentUrl)
            .HasMaxLength(500);

        // Relationships
        builder.HasMany(x => x.StatusHistory)
            .WithOne(x => x.Worker)
            .HasForeignKey(x => x.WorkerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Skills)
            .WithOne(x => x.Worker)
            .HasForeignKey(x => x.WorkerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Languages)
            .WithOne(x => x.Worker)
            .HasForeignKey(x => x.WorkerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.TenantId, x.WorkerCode })
            .IsUnique()
            .HasDatabaseName("ix_workers_tenant_id_worker_code");

        builder.HasIndex(x => new { x.TenantId, x.Status })
            .HasDatabaseName("ix_workers_tenant_id_status");

        builder.HasIndex(x => new { x.TenantId, x.Location })
            .HasDatabaseName("ix_workers_tenant_id_location");

        builder.HasIndex(x => new { x.TenantId, x.CandidateId })
            .IsUnique()
            .HasDatabaseName("ix_workers_tenant_id_candidate_id");

        builder.HasIndex(x => new { x.TenantId, x.Nationality })
            .HasDatabaseName("ix_workers_tenant_id_nationality");

        builder.HasIndex(x => new { x.TenantId, x.JobCategoryId })
            .HasDatabaseName("ix_workers_tenant_id_job_category_id");

        builder.HasIndex(x => new { x.TenantId, x.TenantSupplierId })
            .HasDatabaseName("ix_workers_tenant_id_tenant_supplier_id");
    }
}
