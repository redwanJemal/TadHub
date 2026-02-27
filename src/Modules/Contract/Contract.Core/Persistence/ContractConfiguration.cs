using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Contract.Core.Entities;

namespace Contract.Core.Persistence;

public class ContractConfiguration : IEntityTypeConfiguration<Entities.Contract>
{
    public void Configure(EntityTypeBuilder<Entities.Contract> builder)
    {
        builder.ToTable("contracts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ContractCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.StatusReason)
            .HasMaxLength(500);

        builder.Property(x => x.RatePeriod)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.Rate)
            .HasPrecision(18, 2);

        builder.Property(x => x.TotalValue)
            .HasPrecision(18, 2);

        builder.Property(x => x.TerminationReason)
            .HasMaxLength(500);

        builder.Property(x => x.TerminatedBy)
            .HasMaxLength(20)
            .HasConversion<string?>();

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        // Relationships
        builder.HasMany(x => x.StatusHistory)
            .WithOne(x => x.Contract)
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => new { x.TenantId, x.ContractCode })
            .IsUnique()
            .HasDatabaseName("ix_contracts_tenant_id_contract_code");

        builder.HasIndex(x => new { x.TenantId, x.Status })
            .HasDatabaseName("ix_contracts_tenant_id_status");

        builder.HasIndex(x => new { x.TenantId, x.Type })
            .HasDatabaseName("ix_contracts_tenant_id_type");

        builder.HasIndex(x => new { x.TenantId, x.WorkerId })
            .HasDatabaseName("ix_contracts_tenant_id_worker_id");

        builder.HasIndex(x => new { x.TenantId, x.ClientId })
            .HasDatabaseName("ix_contracts_tenant_id_client_id");

        builder.HasIndex(x => new { x.TenantId, x.StartDate })
            .HasDatabaseName("ix_contracts_tenant_id_start_date");
    }
}
