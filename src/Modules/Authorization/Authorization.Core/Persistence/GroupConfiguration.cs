using Authorization.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization.Core.Persistence;

/// <summary>
/// EF Core configuration for Group entity.
/// </summary>
public class GroupConfiguration : IEntityTypeConfiguration<Group>
{
    public void Configure(EntityTypeBuilder<Group> builder)
    {
        builder.ToTable("groups");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        // Unique constraint on name within tenant
        builder.HasIndex(x => new { x.TenantId, x.Name })
            .IsUnique()
            .HasDatabaseName("ix_groups_tenant_name");

        // Index for tenant queries
        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_groups_tenant_id");
    }
}

/// <summary>
/// EF Core configuration for GroupUser entity.
/// </summary>
public class GroupUserConfiguration : IEntityTypeConfiguration<GroupUser>
{
    public void Configure(EntityTypeBuilder<GroupUser> builder)
    {
        builder.ToTable("group_users");

        builder.HasKey(x => x.Id);

        // Unique constraint on group-user pair
        builder.HasIndex(x => new { x.GroupId, x.UserId })
            .IsUnique()
            .HasDatabaseName("ix_group_users_group_user");

        // Relationships
        builder.HasOne(x => x.Group)
            .WithMany(x => x.Members)
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>
/// EF Core configuration for GroupRole entity.
/// </summary>
public class GroupRoleConfiguration : IEntityTypeConfiguration<GroupRole>
{
    public void Configure(EntityTypeBuilder<GroupRole> builder)
    {
        builder.ToTable("group_roles");

        builder.HasKey(x => x.Id);

        // Unique constraint on group-role pair
        builder.HasIndex(x => new { x.GroupId, x.RoleId })
            .IsUnique()
            .HasDatabaseName("ix_group_roles_group_role");

        // Relationships
        builder.HasOne(x => x.Group)
            .WithMany(x => x.Roles)
            .HasForeignKey(x => x.GroupId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
