using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenancy.Core.Entities;

namespace Tenancy.Core.Persistence;

/// <summary>
/// EF Core configuration for SharedPoolAgreement entity.
/// </summary>
public class SharedPoolAgreementConfiguration : IEntityTypeConfiguration<SharedPoolAgreement>
{
    public void Configure(EntityTypeBuilder<SharedPoolAgreement> builder)
    {
        // Table with check constraints
        builder.ToTable("shared_pool_agreements", t =>
        {
            // Check constraint: from and to tenant must be different
            t.HasCheckConstraint(
                "ck_shared_pool_agreements_different_tenants",
                "from_tenant_id <> to_tenant_id");

            // Check constraint: revenue split between 0 and 100
            t.HasCheckConstraint(
                "ck_shared_pool_agreements_revenue_split",
                "revenue_split_percentage >= 0 AND revenue_split_percentage <= 100");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FromTenantId)
            .IsRequired();

        builder.Property(x => x.ToTenantId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.RevenueSplitPercentage)
            .HasPrecision(5, 2);

        builder.Property(x => x.AgreementDocumentUrl)
            .HasMaxLength(500);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        // Index for from tenant queries
        builder.HasIndex(x => x.FromTenantId)
            .HasDatabaseName("ix_shared_pool_agreements_from_tenant");

        // Index for to tenant queries
        builder.HasIndex(x => x.ToTenantId)
            .HasDatabaseName("ix_shared_pool_agreements_to_tenant");

        // Index for status
        builder.HasIndex(x => x.Status)
            .HasDatabaseName("ix_shared_pool_agreements_status");

        // Unique constraint: only one active agreement per tenant pair
        builder.HasIndex(x => new { x.FromTenantId, x.ToTenantId, x.Status })
            .HasDatabaseName("ix_shared_pool_agreements_tenant_pair_status")
            .HasFilter("status = 'Active'")
            .IsUnique();
    }
}
