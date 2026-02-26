using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Candidate.Core.Entities;

namespace Candidate.Core.Persistence;

/// <summary>
/// EF Core configuration for Candidate entity.
/// </summary>
public class CandidateConfiguration : IEntityTypeConfiguration<Entities.Candidate>
{
    public void Configure(EntityTypeBuilder<Entities.Candidate> builder)
    {
        builder.ToTable("candidates");

        builder.HasKey(x => x.Id);

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

        // Sourcing
        builder.Property(x => x.SourceType)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        // Pipeline
        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.StatusReason)
            .HasMaxLength(500);

        // Document tracking
        builder.Property(x => x.MedicalStatus)
            .HasMaxLength(100);

        builder.Property(x => x.VisaStatus)
            .HasMaxLength(100);

        // Operational
        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.Property(x => x.ExternalReference)
            .HasMaxLength(100);

        // Professional Profile
        builder.Property(x => x.Religion)
            .HasMaxLength(50);

        builder.Property(x => x.MaritalStatus)
            .HasMaxLength(20);

        builder.Property(x => x.EducationLevel)
            .HasMaxLength(50);

        // Media
        builder.Property(x => x.PhotoUrl)
            .HasMaxLength(500);

        builder.Property(x => x.VideoUrl)
            .HasMaxLength(500);

        builder.Property(x => x.PassportDocumentUrl)
            .HasMaxLength(500);

        // Financial
        builder.Property(x => x.ProcurementCost)
            .HasPrecision(18, 2);

        builder.Property(x => x.MonthlySalary)
            .HasPrecision(18, 2);

        // Unique index on (TenantId, PassportNumber) — one passport per tenant, filtered for non-null
        builder.HasIndex(x => new { x.TenantId, x.PassportNumber })
            .IsUnique()
            .HasFilter("passport_number IS NOT NULL")
            .HasDatabaseName("ix_candidates_tenant_id_passport_number");

        // TenantSupplierId is a cross-module reference (Supplier module).
        // No navigation property — module boundary. FK constraint exists at DB level from migration.

        // FK to StatusHistory with Cascade delete
        builder.HasMany(x => x.StatusHistory)
            .WithOne(x => x.Candidate)
            .HasForeignKey(x => x.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK to Skills with Cascade delete
        builder.HasMany(x => x.Skills)
            .WithOne(x => x.Candidate)
            .HasForeignKey(x => x.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK to Languages with Cascade delete
        builder.HasMany(x => x.Languages)
            .WithOne(x => x.Candidate)
            .HasForeignKey(x => x.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for common queries
        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_candidates_tenant_id");

        builder.HasIndex(x => new { x.TenantId, x.Status })
            .HasDatabaseName("ix_candidates_tenant_id_status");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("ix_candidates_status");

        builder.HasIndex(x => x.SourceType)
            .HasDatabaseName("ix_candidates_source_type");

        builder.HasIndex(x => x.Nationality)
            .HasDatabaseName("ix_candidates_nationality");

        builder.HasIndex(x => x.TenantSupplierId)
            .HasDatabaseName("ix_candidates_tenant_supplier_id");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("ix_candidates_created_at");

        builder.HasIndex(x => x.FullNameEn)
            .HasDatabaseName("ix_candidates_full_name_en");

        builder.HasIndex(x => x.JobCategoryId)
            .HasDatabaseName("ix_candidates_job_category_id");

        // Note: Soft delete and tenant query filters are applied globally by AppDbContext.
        // Do NOT add HasQueryFilter here — it would override the global composite filter.
    }
}
