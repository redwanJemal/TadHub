using Identity.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Core.Persistence;

/// <summary>
/// EF Core configuration for UserProfile entity.
/// </summary>
public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("user_profiles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.KeycloakId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(x => x.Phone)
            .HasMaxLength(50);

        builder.Property(x => x.Locale)
            .IsRequired()
            .HasMaxLength(10)
            .HasDefaultValue("en");

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);

        // Unique constraints
        builder.HasIndex(x => x.KeycloakId)
            .IsUnique()
            .HasDatabaseName("ix_user_profiles_keycloak_id");

        builder.HasIndex(x => x.Email)
            .IsUnique()
            .HasDatabaseName("ix_user_profiles_email");

        // Index for common queries
        builder.HasIndex(x => x.IsActive)
            .HasDatabaseName("ix_user_profiles_is_active");

        builder.HasIndex(x => x.CreatedAt)
            .HasDatabaseName("ix_user_profiles_created_at");

        // Ignore computed property
        builder.Ignore(x => x.FullName);
    }
}
