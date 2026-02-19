using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Portal.Core.Persistence;

public class PortalConfiguration : IEntityTypeConfiguration<Entities.Portal>
{
    public void Configure(EntityTypeBuilder<Entities.Portal> builder)
    {
        builder.ToTable("portals");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Subdomain)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(1024);

        // Branding
        builder.Property(x => x.PrimaryColor).HasMaxLength(16);
        builder.Property(x => x.SecondaryColor).HasMaxLength(16);
        builder.Property(x => x.LogoUrl).HasMaxLength(512);
        builder.Property(x => x.FaviconUrl).HasMaxLength(512);
        builder.Property(x => x.CustomCss).HasMaxLength(50000);

        // SEO
        builder.Property(x => x.SeoTitle).HasMaxLength(256);
        builder.Property(x => x.SeoDescription).HasMaxLength(512);
        builder.Property(x => x.SeoKeywords).HasMaxLength(512);
        builder.Property(x => x.OgImageUrl).HasMaxLength(512);

        // SSO
        builder.Property(x => x.SsoConfig).HasColumnType("jsonb");

        // Stripe
        builder.Property(x => x.StripeAccountId).HasMaxLength(256);

        // Unique subdomain globally
        builder.HasIndex(x => x.Subdomain)
            .IsUnique()
            .HasDatabaseName("ix_portals_subdomain");

        // Tenant + active index
        builder.HasIndex(x => new { x.TenantId, x.IsActive })
            .HasDatabaseName("ix_portals_tenant_active");

        builder.HasMany(x => x.Domains)
            .WithOne(x => x.Portal)
            .HasForeignKey(x => x.PortalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Users)
            .WithOne(x => x.Portal)
            .HasForeignKey(x => x.PortalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Pages)
            .WithOne(x => x.Portal)
            .HasForeignKey(x => x.PortalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Settings)
            .WithOne(x => x.Portal)
            .HasForeignKey<Entities.PortalSettings>(x => x.PortalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PortalUserConfiguration : IEntityTypeConfiguration<Entities.PortalUser>
{
    public void Configure(EntityTypeBuilder<Entities.PortalUser> builder)
    {
        builder.ToTable("portal_users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.NormalizedEmail)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.PasswordHash).HasMaxLength(512);
        builder.Property(x => x.FirstName).HasMaxLength(128);
        builder.Property(x => x.LastName).HasMaxLength(128);
        builder.Property(x => x.DisplayName).HasMaxLength(256);
        builder.Property(x => x.AvatarUrl).HasMaxLength(512);
        builder.Property(x => x.PhoneNumber).HasMaxLength(32);
        builder.Property(x => x.SsoSubject).HasMaxLength(256);
        builder.Property(x => x.SsoProvider).HasMaxLength(64);
        builder.Property(x => x.Metadata).HasColumnType("jsonb");

        // Unique email per portal
        builder.HasIndex(x => new { x.PortalId, x.NormalizedEmail })
            .IsUnique()
            .HasDatabaseName("ix_portal_users_portal_email");

        // SSO lookup
        builder.HasIndex(x => new { x.PortalId, x.SsoProvider, x.SsoSubject })
            .HasDatabaseName("ix_portal_users_sso")
            .HasFilter("sso_subject IS NOT NULL");

        builder.HasOne(x => x.Subscription)
            .WithOne(x => x.PortalUser)
            .HasForeignKey<Entities.PortalSubscription>(x => x.PortalUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PortalPageConfiguration : IEntityTypeConfiguration<Entities.PortalPage>
{
    public void Configure(EntityTypeBuilder<Entities.PortalPage> builder)
    {
        builder.ToTable("portal_pages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.Content).HasColumnType("text");
        builder.Property(x => x.ContentFormat).HasMaxLength(16).HasDefaultValue("html");
        builder.Property(x => x.PageType).HasMaxLength(32).HasDefaultValue("content");
        builder.Property(x => x.SeoTitle).HasMaxLength(256);
        builder.Property(x => x.SeoDescription).HasMaxLength(512);
        builder.Property(x => x.FeaturedImageUrl).HasMaxLength(512);

        // Unique slug per portal
        builder.HasIndex(x => new { x.PortalId, x.Slug })
            .IsUnique()
            .HasDatabaseName("ix_portal_pages_portal_slug");
    }
}
