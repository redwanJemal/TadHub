using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tenancy.Core.Entities;

namespace Tenancy.Core.Persistence;

/// <summary>
/// EF Core configuration for TenantUserInvitation entity.
/// </summary>
public class TenantUserInvitationConfiguration : IEntityTypeConfiguration<TenantUserInvitation>
{
    public void Configure(EntityTypeBuilder<TenantUserInvitation> builder)
    {
        builder.ToTable("tenant_user_invitations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Role)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(100);

        // Unique constraint on token
        builder.HasIndex(x => x.Token)
            .IsUnique()
            .HasDatabaseName("ix_tenant_user_invitations_token");

        // Index for tenant queries
        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_tenant_user_invitations_tenant_id");

        // Index for email lookups
        builder.HasIndex(x => x.Email)
            .HasDatabaseName("ix_tenant_user_invitations_email");

        // Index for expiry queries
        builder.HasIndex(x => x.ExpiresAt)
            .HasDatabaseName("ix_tenant_user_invitations_expires_at");

        // Ignore computed properties
        builder.Ignore(x => x.IsExpired);
        builder.Ignore(x => x.IsAccepted);

        // Relationships
        builder.HasOne(x => x.Tenant)
            .WithMany(x => x.Invitations)
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.InvitedBy)
            .WithMany()
            .HasForeignKey(x => x.InvitedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
