using ApiManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ApiManagement.Core.Persistence;

public class ApiKeyConfiguration : IEntityTypeConfiguration<ApiKey>
{
    public void Configure(EntityTypeBuilder<ApiKey> builder)
    {
        builder.ToTable("api_keys");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Prefix).HasMaxLength(16).IsRequired();
        builder.Property(x => x.KeyHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Permissions).HasColumnType("jsonb");

        builder.HasIndex(x => x.KeyHash)
            .IsUnique()
            .HasDatabaseName("ix_api_keys_hash");

        builder.HasIndex(x => new { x.TenantId, x.IsActive })
            .HasDatabaseName("ix_api_keys_tenant_active");

        builder.HasMany(x => x.Logs)
            .WithOne(x => x.ApiKey)
            .HasForeignKey(x => x.ApiKeyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ApiKeyLogConfiguration : IEntityTypeConfiguration<ApiKeyLog>
{
    public void Configure(EntityTypeBuilder<ApiKeyLog> builder)
    {
        builder.ToTable("api_key_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Endpoint).HasMaxLength(512).IsRequired();
        builder.Property(x => x.Method).HasMaxLength(16).IsRequired();
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(512);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2048);

        builder.HasIndex(x => new { x.ApiKeyId, x.CreatedAt })
            .HasDatabaseName("ix_api_key_logs_key_created");

        builder.HasIndex(x => new { x.TenantId, x.StatusCode, x.CreatedAt })
            .HasDatabaseName("ix_api_key_logs_tenant_status_created");
    }
}
