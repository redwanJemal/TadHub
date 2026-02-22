using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenancy.Core.Entities;

namespace Tenancy.Core.Persistence;

/// <summary>
/// EF Core configuration for TenantMembership entity.
/// </summary>
public class TenantMembershipConfiguration : IEntityTypeConfiguration<TenantMembership>
{
    public void Configure(EntityTypeBuilder<TenantMembership> builder)
    {
        builder.ToTable("tenant_memberships");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Unique constraint: user can only be member once per tenant
        builder.HasIndex(x => new { x.TenantId, x.UserId })
            .IsUnique()
            .HasDatabaseName("ix_tenant_memberships_tenant_user");

        // Index for querying user's tenants
        builder.HasIndex(x => x.UserId)
            .HasDatabaseName("ix_tenant_memberships_user_id");

        // Index for querying tenant's members
        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_tenant_memberships_tenant_id");

        // Index on IsOwner for "last owner" guard queries
        builder.HasIndex(x => x.IsOwner)
            .HasDatabaseName("ix_tenant_memberships_is_owner");

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
