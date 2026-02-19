using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenancy.Core.Entities;

namespace Tenancy.Core.Persistence;

/// <summary>
/// EF Core configuration for TenantUser entity.
/// </summary>
public class TenantUserConfiguration : IEntityTypeConfiguration<TenantUser>
{
    public void Configure(EntityTypeBuilder<TenantUser> builder)
    {
        builder.ToTable("tenant_users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Role)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Unique constraint: user can only be member once per tenant
        builder.HasIndex(x => new { x.TenantId, x.UserId })
            .IsUnique()
            .HasDatabaseName("ix_tenant_users_tenant_user");

        // Index for querying user's tenants
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("ix_tenant_users_user_id");

        // Index for querying tenant's members
        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_tenant_users_tenant_id");

        // Index for role queries
        builder.HasIndex(x => x.Role)
            .HasDatabaseName("ix_tenant_users_role");

        // Relationships
        builder.HasOne(x => x.Tenant)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
