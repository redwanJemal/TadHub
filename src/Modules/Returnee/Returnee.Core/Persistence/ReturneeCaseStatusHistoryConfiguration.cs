using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Returnee.Core.Entities;

namespace Returnee.Core.Persistence;

public class ReturneeCaseStatusHistoryConfiguration : IEntityTypeConfiguration<ReturneeCaseStatusHistory>
{
    public void Configure(EntityTypeBuilder<ReturneeCaseStatusHistory> builder)
    {
        builder.ToTable("returnee_case_status_history");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FromStatus)
            .HasMaxLength(30)
            .HasConversion<string?>();

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

        // Indexes
        builder.HasIndex(x => new { x.ReturneeCaseId, x.ChangedAt })
            .HasDatabaseName("ix_returnee_history_case");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_returnee_history_tenant");
    }
}
