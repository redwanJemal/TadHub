using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Financial.Core.Entities;

namespace Financial.Core.Persistence;

public class DiscountProgramConfiguration : IEntityTypeConfiguration<DiscountProgram>
{
    public void Configure(EntityTypeBuilder<DiscountProgram> builder)
    {
        builder.ToTable("discount_programs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.NameAr)
            .HasMaxLength(200);

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.DiscountPercentage).HasPrecision(18, 2);
        builder.Property(x => x.MaxDiscountAmount).HasPrecision(18, 2);

        builder.Property(x => x.Description).HasMaxLength(1000);

        // Indexes
        builder.HasIndex(x => new { x.TenantId, x.Name })
            .HasDatabaseName("ix_discount_programs_tenant_id_name");

        builder.HasIndex(x => new { x.TenantId, x.Type })
            .HasDatabaseName("ix_discount_programs_tenant_id_type");

        builder.HasIndex(x => new { x.TenantId, x.IsActive })
            .HasDatabaseName("ix_discount_programs_tenant_id_is_active");
    }
}
