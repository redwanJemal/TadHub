using Document.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Document.Core.Persistence;

public class WorkerDocumentConfiguration : IEntityTypeConfiguration<WorkerDocument>
{
    public void Configure(EntityTypeBuilder<WorkerDocument> builder)
    {
        builder.ToTable("worker_documents");

        builder.Property(x => x.DocumentType)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.DocumentNumber)
            .HasMaxLength(100);

        builder.Property(x => x.IssuingAuthority)
            .HasMaxLength(200);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.Property(x => x.FileUrl)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(x => new { x.TenantId, x.WorkerId })
            .HasDatabaseName("ix_worker_documents_tenant_id_worker_id");

        builder.HasIndex(x => new { x.TenantId, x.ExpiresAt })
            .HasDatabaseName("ix_worker_documents_tenant_id_expires_at");

        builder.HasIndex(x => new { x.TenantId, x.Status })
            .HasDatabaseName("ix_worker_documents_tenant_id_status");

        builder.HasIndex(x => new { x.TenantId, x.WorkerId, x.DocumentType, x.DocumentNumber })
            .IsUnique()
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ix_worker_documents_tenant_worker_type_number");

        // Ignore computed properties
        builder.Ignore(x => x.DaysUntilExpiry);
        builder.Ignore(x => x.IsExpired);
    }
}
