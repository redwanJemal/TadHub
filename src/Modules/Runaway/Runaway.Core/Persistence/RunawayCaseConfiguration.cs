using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Runaway.Core.Entities;

namespace Runaway.Core.Persistence;

public class RunawayCaseConfiguration : IEntityTypeConfiguration<RunawayCase>
{
    public void Configure(EntityTypeBuilder<RunawayCase> builder)
    {
        builder.ToTable("runaway_cases");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CaseCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.ReportedBy)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.LastKnownLocation)
            .HasMaxLength(500);

        builder.Property(x => x.PoliceReportNumber)
            .HasMaxLength(100);

        builder.Property(x => x.GuaranteePeriodType)
            .HasMaxLength(30)
            .HasConversion<string?>();

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        // Relationships
        builder.HasMany(x => x.Expenses)
            .WithOne(x => x.RunawayCase)
            .HasForeignKey(x => x.RunawayCaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.StatusHistory)
            .WithOne(x => x.RunawayCase)
            .HasForeignKey(x => x.RunawayCaseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.TenantId, x.CaseCode })
            .IsUnique()
            .HasDatabaseName("ix_runaway_cases_tenant_code");

        builder.HasIndex(x => new { x.TenantId, x.Status })
            .HasDatabaseName("ix_runaway_cases_tenant_status");

        builder.HasIndex(x => new { x.TenantId, x.WorkerId })
            .HasDatabaseName("ix_runaway_cases_tenant_worker");

        builder.HasIndex(x => new { x.TenantId, x.ClientId })
            .HasDatabaseName("ix_runaway_cases_tenant_client");

        builder.HasIndex(x => new { x.TenantId, x.ContractId })
            .HasDatabaseName("ix_runaway_cases_tenant_contract");
    }
}
