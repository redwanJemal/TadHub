using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Arrival.Core.Entities;

namespace Arrival.Core.Persistence;

public class ArrivalConfiguration : IEntityTypeConfiguration<Entities.Arrival>
{
    public void Configure(EntityTypeBuilder<Entities.Arrival> builder)
    {
        builder.ToTable("arrivals");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ArrivalCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.FlightNumber)
            .HasMaxLength(50);

        builder.Property(x => x.AirportCode)
            .HasMaxLength(10);

        builder.Property(x => x.AirportName)
            .HasMaxLength(200);

        builder.Property(x => x.PreTravelPhotoUrl)
            .HasMaxLength(500);

        builder.Property(x => x.ArrivalPhotoUrl)
            .HasMaxLength(500);

        builder.Property(x => x.DriverPickupPhotoUrl)
            .HasMaxLength(500);

        builder.Property(x => x.DriverName)
            .HasMaxLength(200);

        builder.Property(x => x.AccommodationConfirmedBy)
            .HasMaxLength(200);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        // Relationships
        builder.HasMany(x => x.StatusHistory)
            .WithOne(x => x.Arrival)
            .HasForeignKey(x => x.ArrivalId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.TenantId, x.ArrivalCode })
            .IsUnique()
            .HasDatabaseName("ix_arrivals_tenant_code");

        builder.HasIndex(x => new { x.TenantId, x.Status })
            .HasDatabaseName("ix_arrivals_tenant_status");

        builder.HasIndex(x => new { x.TenantId, x.PlacementId })
            .HasDatabaseName("ix_arrivals_tenant_placement");

        builder.HasIndex(x => new { x.TenantId, x.WorkerId })
            .HasDatabaseName("ix_arrivals_tenant_worker");

        builder.HasIndex(x => new { x.TenantId, x.DriverId })
            .HasDatabaseName("ix_arrivals_tenant_driver");

        builder.HasIndex(x => new { x.TenantId, x.ScheduledArrivalDate })
            .HasDatabaseName("ix_arrivals_tenant_scheduled_date");
    }
}
