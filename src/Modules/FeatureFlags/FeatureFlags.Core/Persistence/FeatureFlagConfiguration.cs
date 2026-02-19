using FeatureFlags.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FeatureFlags.Core.Persistence;

public class FeatureFlagConfiguration : IEntityTypeConfiguration<FeatureFlag>
{
    public void Configure(EntityTypeBuilder<FeatureFlag> builder)
    {
        builder.ToTable("feature_flags");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(512);
        builder.Property(x => x.AllowedPlans).HasColumnType("jsonb");
        builder.Property(x => x.AllowedTenantIds).HasColumnType("jsonb");
        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("ix_feature_flags_name");
        builder.HasMany(x => x.Filters).WithOne(x => x.FeatureFlag).HasForeignKey(x => x.FeatureFlagId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class FeatureFlagFilterConfiguration : IEntityTypeConfiguration<FeatureFlagFilter>
{
    public void Configure(EntityTypeBuilder<FeatureFlagFilter> builder)
    {
        builder.ToTable("feature_flag_filters");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(256).IsRequired();
    }
}
