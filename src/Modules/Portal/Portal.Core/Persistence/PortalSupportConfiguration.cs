using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portal.Core.Entities;

namespace Portal.Core.Persistence;

public class PortalDomainConfiguration : IEntityTypeConfiguration<PortalDomain>
{
    public void Configure(EntityTypeBuilder<PortalDomain> builder)
    {
        builder.ToTable("portal_domains");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Domain)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(x => x.VerificationToken).HasMaxLength(128);
        builder.Property(x => x.SslStatus).HasMaxLength(32).HasDefaultValue("pending");

        // Unique domain globally
        builder.HasIndex(x => x.Domain)
            .IsUnique()
            .HasDatabaseName("ix_portal_domains_domain");

        builder.HasIndex(x => x.PortalId)
            .HasDatabaseName("ix_portal_domains_portal");
    }
}

public class PortalSettingsConfiguration : IEntityTypeConfiguration<PortalSettings>
{
    public void Configure(EntityTypeBuilder<PortalSettings> builder)
    {
        builder.ToTable("portal_settings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DefaultLanguage).HasMaxLength(8).HasDefaultValue("en");
        builder.Property(x => x.Timezone).HasMaxLength(64).HasDefaultValue("UTC");
        builder.Property(x => x.DateFormat).HasMaxLength(32);
        builder.Property(x => x.SupportEmail).HasMaxLength(256);
        builder.Property(x => x.FromEmail).HasMaxLength(256);
        builder.Property(x => x.FromName).HasMaxLength(128);
        builder.Property(x => x.TwitterUrl).HasMaxLength(256);
        builder.Property(x => x.LinkedInUrl).HasMaxLength(256);
        builder.Property(x => x.FacebookUrl).HasMaxLength(256);
        builder.Property(x => x.InstagramUrl).HasMaxLength(256);
        builder.Property(x => x.TermsUrl).HasMaxLength(512);
        builder.Property(x => x.PrivacyUrl).HasMaxLength(512);
        builder.Property(x => x.GoogleAnalyticsId).HasMaxLength(32);
        builder.Property(x => x.CustomTrackingScripts).HasColumnType("jsonb");
        builder.Property(x => x.AdditionalSettings).HasColumnType("jsonb");

        builder.HasIndex(x => x.PortalId)
            .IsUnique()
            .HasDatabaseName("ix_portal_settings_portal");
    }
}

public class PortalThemeConfiguration : IEntityTypeConfiguration<PortalTheme>
{
    public void Configure(EntityTypeBuilder<PortalTheme> builder)
    {
        builder.ToTable("portal_themes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.BaseTemplate).HasMaxLength(32).HasDefaultValue("default");
        builder.Property(x => x.Variables).HasColumnType("jsonb");
        builder.Property(x => x.CustomCss).HasColumnType("text");
        builder.Property(x => x.CustomJs).HasColumnType("text");
        builder.Property(x => x.HeaderTemplate).HasColumnType("text");
        builder.Property(x => x.FooterTemplate).HasColumnType("text");

        builder.HasIndex(x => new { x.PortalId, x.IsActive })
            .HasDatabaseName("ix_portal_themes_portal_active");
    }
}

public class PortalSubscriptionConfiguration : IEntityTypeConfiguration<PortalSubscription>
{
    public void Configure(EntityTypeBuilder<PortalSubscription> builder)
    {
        builder.ToTable("portal_subscriptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PlanName).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.StripeSubscriptionId).HasMaxLength(256);
        builder.Property(x => x.StripeCustomerId).HasMaxLength(256);

        builder.HasIndex(x => x.PortalUserId)
            .IsUnique()
            .HasDatabaseName("ix_portal_subscriptions_user");

        builder.HasIndex(x => x.StripeSubscriptionId)
            .HasDatabaseName("ix_portal_subscriptions_stripe");
    }
}

public class PortalInvitationConfiguration : IEntityTypeConfiguration<PortalInvitation>
{
    public void Configure(EntityTypeBuilder<PortalInvitation> builder)
    {
        builder.ToTable("portal_invitations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Token).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(1024);

        builder.HasIndex(x => x.Token)
            .IsUnique()
            .HasDatabaseName("ix_portal_invitations_token");

        builder.HasIndex(x => new { x.PortalId, x.Email, x.Status })
            .HasDatabaseName("ix_portal_invitations_portal_email");
    }
}

public class PortalApiKeyConfiguration : IEntityTypeConfiguration<PortalApiKey>
{
    public void Configure(EntityTypeBuilder<PortalApiKey> builder)
    {
        builder.ToTable("portal_api_keys");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.KeyPrefix).HasMaxLength(16).IsRequired();
        builder.Property(x => x.KeyHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Scopes).HasColumnType("jsonb");
        builder.Property(x => x.AllowedIps).HasColumnType("jsonb");

        builder.HasIndex(x => x.KeyHash)
            .IsUnique()
            .HasDatabaseName("ix_portal_api_keys_hash");

        builder.HasIndex(x => new { x.PortalId, x.IsActive })
            .HasDatabaseName("ix_portal_api_keys_portal_active");
    }
}

public class PortalUserRegistrationConfiguration : IEntityTypeConfiguration<PortalUserRegistration>
{
    public void Configure(EntityTypeBuilder<PortalUserRegistration> builder)
    {
        builder.ToTable("portal_user_registrations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(512).IsRequired();
        builder.Property(x => x.FirstName).HasMaxLength(128);
        builder.Property(x => x.LastName).HasMaxLength(128);
        builder.Property(x => x.VerificationToken).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(512);

        builder.HasIndex(x => x.VerificationToken)
            .IsUnique()
            .HasDatabaseName("ix_portal_user_registrations_token");

        builder.HasIndex(x => new { x.PortalId, x.Email, x.Status })
            .HasDatabaseName("ix_portal_user_registrations_portal_email");
    }
}
