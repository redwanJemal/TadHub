using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Client.Core.Persistence;

public class ClientConfiguration : IEntityTypeConfiguration<Entities.Client>
{
    public void Configure(EntityTypeBuilder<Entities.Client> builder)
    {
        builder.ToTable("clients");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.NameEn).IsRequired().HasMaxLength(256);
        builder.Property(x => x.NameAr).HasMaxLength(256);
        builder.Property(x => x.NationalId).HasMaxLength(50);
        builder.Property(x => x.Phone).HasMaxLength(50);
        builder.Property(x => x.Email).HasMaxLength(256);
        builder.Property(x => x.Address).HasMaxLength(512);
        builder.Property(x => x.City).HasMaxLength(128);
        builder.Property(x => x.Notes).HasMaxLength(2048);
        builder.Property(x => x.IsActive).HasDefaultValue(true);

        builder.HasIndex(x => new { x.TenantId, x.NameEn })
            .IsUnique()
            .HasDatabaseName("ix_clients_tenant_id_name_en");

        builder.HasIndex(x => new { x.TenantId, x.NationalId })
            .IsUnique()
            .HasFilter("national_id IS NOT NULL")
            .HasDatabaseName("ix_clients_tenant_id_national_id");

        builder.HasIndex(x => new { x.TenantId, x.IsActive })
            .HasDatabaseName("ix_clients_tenant_id_is_active");
    }
}
