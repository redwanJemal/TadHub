using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SupplierPortal.Core.Entities;

namespace SupplierPortal.Core.Persistence;

public class SupplierUserConfiguration : IEntityTypeConfiguration<SupplierUser>
{
    public void Configure(EntityTypeBuilder<SupplierUser> builder)
    {
        builder.ToTable("supplier_users");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.SupplierId)
            .IsRequired();

        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);

        builder.Property(e => e.DisplayName)
            .HasMaxLength(200);

        builder.Property(e => e.Email)
            .HasMaxLength(200);

        builder.Property(e => e.Phone)
            .HasMaxLength(50);

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("ix_supplier_users_user_id")
            .IsUnique();

        builder.HasIndex(e => e.SupplierId)
            .HasDatabaseName("ix_supplier_users_supplier_id");
    }
}
