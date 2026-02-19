using Authorization.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization.Core.Persistence;

/// <summary>
/// EF Core configuration for UserRole entity.
/// </summary>
public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles");

        builder.HasKey(x => x.Id);

        // Unique constraint on user-role within tenant
        builder.HasIndex(x => new { x.TenantId, x.UserId, x.RoleId })
            .IsUnique()
            .HasDatabaseName("ix_user_roles_tenant_user_role");

        // Index for user queries
        builder.HasIndex(x => new { x.TenantId, x.UserId })
            .HasDatabaseName("ix_user_roles_tenant_user");

        // Index for role queries
        builder.HasIndex(x => x.RoleId)
            .HasDatabaseName("ix_user_roles_role_id");

        // Relationships
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role)
            .WithMany(x => x.UserRoles)
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
