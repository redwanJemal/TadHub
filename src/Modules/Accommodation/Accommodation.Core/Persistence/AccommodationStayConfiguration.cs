using Accommodation.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Accommodation.Core.Persistence;

public class AccommodationStayConfiguration : IEntityTypeConfiguration<AccommodationStay>
{
    public void Configure(EntityTypeBuilder<AccommodationStay> builder)
    {
        builder.ToTable("accommodation_stays");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.StayCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.DepartureReason)
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.Room)
            .HasMaxLength(50);

        builder.Property(x => x.Location)
            .HasMaxLength(200);

        builder.Property(x => x.DepartureNotes)
            .HasMaxLength(1000);

        builder.Property(x => x.CheckedInBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.CheckedOutBy)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(x => new { x.TenantId, x.StayCode })
            .IsUnique()
            .HasDatabaseName("ix_accommodation_stays_tenant_code");

        builder.HasIndex(x => new { x.TenantId, x.Status })
            .HasDatabaseName("ix_accommodation_stays_tenant_status");

        builder.HasIndex(x => new { x.TenantId, x.WorkerId })
            .HasDatabaseName("ix_accommodation_stays_tenant_worker");

        builder.HasIndex(x => new { x.TenantId, x.CheckInDate })
            .HasDatabaseName("ix_accommodation_stays_tenant_checkin_date");
    }
}
