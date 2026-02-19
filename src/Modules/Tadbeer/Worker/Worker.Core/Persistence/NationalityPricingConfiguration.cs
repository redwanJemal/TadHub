using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Worker.Core.Entities;

namespace Worker.Core.Persistence;

/// <summary>
/// EF Core configuration for NationalityPricing entity.
/// </summary>
public class NationalityPricingConfiguration : IEntityTypeConfiguration<NationalityPricing>
{
    public void Configure(EntityTypeBuilder<NationalityPricing> builder)
    {
        builder.ToTable("nationality_pricing");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nationality)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.ContractType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(3)
            .HasDefaultValue("AED");

        builder.Property(x => x.EffectiveFrom)
            .IsRequired();

        // Check constraint for positive amount
        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "ck_nationality_pricing_amount",
                "amount > 0");
        });

        // Composite index for price lookups
        builder.HasIndex(x => new { x.TenantId, x.Nationality, x.ContractType, x.EffectiveFrom })
            .HasDatabaseName("ix_nationality_pricing_lookup");

        // Index for nationality filtering
        builder.HasIndex(x => x.Nationality)
            .HasDatabaseName("ix_nationality_pricing_nationality");

        // Index for contract type filtering
        builder.HasIndex(x => x.ContractType)
            .HasDatabaseName("ix_nationality_pricing_contract_type");

        // Index for effective date range queries
        builder.HasIndex(x => new { x.EffectiveFrom, x.EffectiveTo })
            .HasDatabaseName("ix_nationality_pricing_effective_range");
    }
}
