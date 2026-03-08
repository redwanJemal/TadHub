using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Visa.Core.Entities;

namespace Visa.Core.Persistence;

public class VisaApplicationStatusHistoryConfiguration : IEntityTypeConfiguration<VisaApplicationStatusHistory>
{
    public void Configure(EntityTypeBuilder<VisaApplicationStatusHistory> builder)
    {
        builder.ToTable("visa_application_status_history");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FromStatus)
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.ToStatus)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.ChangedBy)
            .HasMaxLength(200);

        builder.Property(x => x.Reason)
            .HasMaxLength(500);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(x => x.VisaApplicationId)
            .HasDatabaseName("ix_visa_application_status_history_application");
    }
}
