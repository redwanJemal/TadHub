using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Contract.Core.Entities;

namespace Contract.Core.Persistence;

public class ContractStatusHistoryConfiguration : IEntityTypeConfiguration<ContractStatusHistory>
{
    public void Configure(EntityTypeBuilder<ContractStatusHistory> builder)
    {
        builder.ToTable("contract_status_history");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FromStatus)
            .HasMaxLength(30)
            .HasConversion<string?>();

        builder.Property(x => x.ToStatus)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.Reason)
            .HasMaxLength(500);

        builder.Property(x => x.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(x => new { x.ContractId, x.ChangedAt })
            .HasDatabaseName("ix_contract_status_history_contract_id_changed_at");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_contract_status_history_tenant_id");
    }
}
