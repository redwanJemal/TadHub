using ClientManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClientManagement.Core.Persistence;

/// <summary>
/// EF Core configuration for DiscountCard entity.
/// </summary>
public class DiscountCardConfiguration : IEntityTypeConfiguration<DiscountCard>
{
    public void Configure(EntityTypeBuilder<DiscountCard> builder)
    {
        builder.ToTable("discount_cards");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClientId)
            .IsRequired();

        builder.Property(x => x.CardType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.CardNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.DiscountPercentage)
            .HasPrecision(5, 2)
            .IsRequired();

        // Check constraint for percentage
        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                "ck_discount_cards_percentage",
                "discount_percentage >= 0 AND discount_percentage <= 100");
        });

        // Index for client cards
        builder.HasIndex(x => x.ClientId)
            .HasDatabaseName("ix_discount_cards_client_id");

        // Index for card type
        builder.HasIndex(x => x.CardType)
            .HasDatabaseName("ix_discount_cards_card_type");

        // Unique card number within type
        builder.HasIndex(x => new { x.CardType, x.CardNumber })
            .IsUnique()
            .HasDatabaseName("ix_discount_cards_type_number");
    }
}
