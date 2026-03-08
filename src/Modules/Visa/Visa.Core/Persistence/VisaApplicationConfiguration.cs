using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Visa.Core.Entities;

namespace Visa.Core.Persistence;

public class VisaApplicationConfiguration : IEntityTypeConfiguration<VisaApplication>
{
    public void Configure(EntityTypeBuilder<VisaApplication> builder)
    {
        builder.ToTable("visa_applications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ApplicationCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.VisaType)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.StatusReason)
            .HasMaxLength(500);

        builder.Property(x => x.ReferenceNumber)
            .HasMaxLength(100);

        builder.Property(x => x.VisaNumber)
            .HasMaxLength(100);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.Property(x => x.RejectionReason)
            .HasMaxLength(1000);

        // Relationships
        builder.HasMany(x => x.StatusHistory)
            .WithOne(x => x.VisaApplication)
            .HasForeignKey(x => x.VisaApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Documents)
            .WithOne(x => x.VisaApplication)
            .HasForeignKey(x => x.VisaApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.TenantId, x.ApplicationCode })
            .IsUnique()
            .HasDatabaseName("ix_visa_applications_tenant_code");

        builder.HasIndex(x => new { x.TenantId, x.Status })
            .HasDatabaseName("ix_visa_applications_tenant_status");

        builder.HasIndex(x => new { x.TenantId, x.WorkerId })
            .HasDatabaseName("ix_visa_applications_tenant_worker");

        builder.HasIndex(x => new { x.TenantId, x.ClientId })
            .HasDatabaseName("ix_visa_applications_tenant_client");

        builder.HasIndex(x => new { x.TenantId, x.PlacementId })
            .HasDatabaseName("ix_visa_applications_tenant_placement");

        builder.HasIndex(x => new { x.TenantId, x.VisaType })
            .HasDatabaseName("ix_visa_applications_tenant_type");
    }
}
