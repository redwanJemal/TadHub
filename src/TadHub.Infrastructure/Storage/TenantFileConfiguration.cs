using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TadHub.Infrastructure.Storage;

public class TenantFileConfiguration : IEntityTypeConfiguration<TenantFile>
{
    public void Configure(EntityTypeBuilder<TenantFile> builder)
    {
        builder.ToTable("tenant_files");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OriginalFileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.StorageKey)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.FileType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.EntityType)
            .HasMaxLength(100);

        // Tenant isolation index
        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_tenant_files_tenant_id");

        // Lookup by linked entity
        builder.HasIndex(x => new { x.EntityType, x.EntityId })
            .HasDatabaseName("ix_tenant_files_entity_type_entity_id");

        // Orphan cleanup: find unattached files older than threshold
        builder.HasIndex(x => new { x.IsAttached, x.CreatedAt })
            .HasDatabaseName("ix_tenant_files_is_attached_created_at");
    }
}
