using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Subscription.Core.Entities;

namespace Subscription.Core.Persistence;

public class CreditConfiguration : IEntityTypeConfiguration<Credit>
{
    public void Configure(EntityTypeBuilder<Credit> builder)
    {
        builder.ToTable("credits");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.ReferenceId)
            .HasMaxLength(256);

        builder.Property(x => x.ReferenceType)
            .HasMaxLength(64);

        builder.Property(x => x.Metadata)
            .HasColumnType("jsonb");

        // Index for tenant ledger queries
        builder.HasIndex(x => new { x.TenantId, x.CreatedAt })
            .HasDatabaseName("ix_credits_tenant_created");

        // Index for type filtering
        builder.HasIndex(x => new { x.TenantId, x.Type, x.CreatedAt })
            .HasDatabaseName("ix_credits_tenant_type_created");

        // Index for expiring credits
        builder.HasIndex(x => x.ExpiresAt)
            .HasDatabaseName("ix_credits_expires_at")
            .HasFilter("expires_at IS NOT NULL");
    }
}
