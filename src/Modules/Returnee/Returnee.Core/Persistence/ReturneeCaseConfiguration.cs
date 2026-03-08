using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Returnee.Core.Entities;

namespace Returnee.Core.Persistence;

public class ReturneeCaseConfiguration : IEntityTypeConfiguration<ReturneeCase>
{
    public void Configure(EntityTypeBuilder<ReturneeCase> builder)
    {
        builder.ToTable("returnee_cases");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CaseCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.ReturnType)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.ReturnReason)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(x => x.GuaranteePeriodType)
            .HasMaxLength(30)
            .HasConversion<string?>();

        builder.Property(x => x.TotalAmountPaid)
            .HasPrecision(18, 2);

        builder.Property(x => x.RefundAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.ApprovedBy)
            .HasMaxLength(200);

        builder.Property(x => x.RejectedReason)
            .HasMaxLength(2000);

        builder.Property(x => x.SettlementNotes)
            .HasMaxLength(2000);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        // Relationships
        builder.HasMany(x => x.Expenses)
            .WithOne(x => x.ReturneeCase)
            .HasForeignKey(x => x.ReturneeCaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.StatusHistory)
            .WithOne(x => x.ReturneeCase)
            .HasForeignKey(x => x.ReturneeCaseId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.TenantId, x.CaseCode })
            .IsUnique()
            .HasDatabaseName("ix_returnee_cases_tenant_code");

        builder.HasIndex(x => new { x.TenantId, x.Status })
            .HasDatabaseName("ix_returnee_cases_tenant_status");

        builder.HasIndex(x => new { x.TenantId, x.WorkerId })
            .HasDatabaseName("ix_returnee_cases_tenant_worker");

        builder.HasIndex(x => new { x.TenantId, x.ClientId })
            .HasDatabaseName("ix_returnee_cases_tenant_client");

        builder.HasIndex(x => new { x.TenantId, x.ContractId })
            .HasDatabaseName("ix_returnee_cases_tenant_contract");
    }
}
