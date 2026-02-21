using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReferenceData.Core.Entities;
using TadHub.SharedKernel.Localization;

namespace ReferenceData.Core.Persistence;

/// <summary>
/// EF Core configuration for Country entity.
/// Global entity (not tenant-scoped).
/// </summary>
public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("countries");

        builder.HasKey(x => x.Id);

        // ISO codes
        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(2);

        builder.Property(x => x.Alpha3Code)
            .IsRequired()
            .HasMaxLength(3);

        // Localized name stored as JSONB
        builder.Property(x => x.Name)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<LocalizedString>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new LocalizedString())
            .HasColumnType("jsonb")
            .IsRequired();

        // Localized nationality stored as JSONB
        builder.Property(x => x.Nationality)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => System.Text.Json.JsonSerializer.Deserialize<LocalizedString>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new LocalizedString())
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.DialingCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(x => x.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(100);

        builder.Property(x => x.IsCommonNationality)
            .IsRequired()
            .HasDefaultValue(false);

        // Unique ISO code
        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasDatabaseName("ix_countries_code");

        builder.HasIndex(x => x.Alpha3Code)
            .IsUnique()
            .HasDatabaseName("ix_countries_alpha3_code");

        // Index for dropdown queries
        builder.HasIndex(x => new { x.IsActive, x.DisplayOrder })
            .HasDatabaseName("ix_countries_active_order");

        // Index for common nationalities
        builder.HasIndex(x => x.IsCommonNationality)
            .HasFilter("\"IsCommonNationality\" = true")
            .HasDatabaseName("ix_countries_common_nationality");
    }
}
