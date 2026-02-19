using ClientManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClientManagement.Core.Persistence;

/// <summary>
/// EF Core configuration for ClientDocument entity.
/// </summary>
public class ClientDocumentConfiguration : IEntityTypeConfiguration<ClientDocument>
{
    public void Configure(EntityTypeBuilder<ClientDocument> builder)
    {
        builder.ToTable("client_documents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClientId)
            .IsRequired();

        builder.Property(x => x.DocumentType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.FileUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.UploadedAt)
            .IsRequired();

        // Index for client + document type queries
        builder.HasIndex(x => new { x.ClientId, x.DocumentType })
            .HasDatabaseName("ix_client_documents_client_type");

        // Index for expiry checks
        builder.HasIndex(x => x.ExpiresAt)
            .HasDatabaseName("ix_client_documents_expires_at")
            .HasFilter("expires_at IS NOT NULL");

        // Index for verification status
        builder.HasIndex(x => x.IsVerified)
            .HasDatabaseName("ix_client_documents_is_verified");
    }
}
