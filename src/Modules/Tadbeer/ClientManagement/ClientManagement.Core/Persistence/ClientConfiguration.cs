using ClientManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClientManagement.Core.Persistence;

/// <summary>
/// EF Core configuration for Client entity.
/// </summary>
public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EmiratesId)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.FullNameEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.FullNameAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.PassportNumber)
            .HasMaxLength(20);

        builder.Property(x => x.Nationality)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Phone)
            .HasMaxLength(20);

        builder.Property(x => x.Email)
            .HasMaxLength(200);

        builder.Property(x => x.Category)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.SponsorFileStatus)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Emirate)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.BlockedReason)
            .HasMaxLength(500);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        // Unique Emirates ID within tenant
        builder.HasIndex(x => new { x.TenantId, x.EmiratesId })
            .IsUnique()
            .HasDatabaseName("ix_clients_tenant_emirates_id");

        // Index for filtering
        builder.HasIndex(x => x.Category)
            .HasDatabaseName("ix_clients_category");

        builder.HasIndex(x => x.SponsorFileStatus)
            .HasDatabaseName("ix_clients_sponsor_file_status");

        builder.HasIndex(x => x.IsVerified)
            .HasDatabaseName("ix_clients_is_verified");

        builder.HasIndex(x => x.Nationality)
            .HasDatabaseName("ix_clients_nationality");

        builder.HasIndex(x => x.Emirate)
            .HasDatabaseName("ix_clients_emirate");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("ix_clients_created_at");

        // Full-text search on names
        builder.HasIndex(x => x.FullNameEn)
            .HasDatabaseName("ix_clients_full_name_en");

        builder.HasIndex(x => x.FullNameAr)
            .HasDatabaseName("ix_clients_full_name_ar");

        // Relationships
        builder.HasMany(x => x.Documents)
            .WithOne(x => x.Client)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.CommunicationLogs)
            .WithOne(x => x.Client)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.DiscountCards)
            .WithOne(x => x.Client)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Leads)
            .WithOne(x => x.Client)
            .HasForeignKey(x => x.ClientId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
