using ClientManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClientManagement.Core.Persistence;

/// <summary>
/// EF Core configuration for Lead entity.
/// </summary>
public class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("leads");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Source)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.ContactName)
            .HasMaxLength(200);

        builder.Property(x => x.ContactPhone)
            .HasMaxLength(20);

        builder.Property(x => x.ContactEmail)
            .HasMaxLength(200);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        // Index for status filtering (array filters)
        builder.HasIndex(x => x.Status)
            .HasDatabaseName("ix_leads_status");

        // Index for source filtering
        builder.HasIndex(x => x.Source)
            .HasDatabaseName("ix_leads_source");

        // Index for assignment
        builder.HasIndex(x => x.AssignedToUserId)
            .HasDatabaseName("ix_leads_assigned_to")
            .HasFilter("assigned_to_user_id IS NOT NULL");

        // Index for created date
        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("ix_leads_created_at");

        // Index for converted leads
        builder.HasIndex(x => x.ClientId)
            .HasDatabaseName("ix_leads_client_id")
            .HasFilter("client_id IS NOT NULL");
    }
}
