using _Template.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace _Template.Core.Persistence;

public class TemplateEntityConfiguration : IEntityTypeConfiguration<TemplateEntity>
{
    public void Configure(EntityTypeBuilder<TemplateEntity> builder)
    {
        builder.ToTable("template_entities");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1024);
        builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique().HasDatabaseName("ix_template_entities_tenant_name");
        builder.HasIndex(x => new { x.TenantId, x.IsActive }).HasDatabaseName("ix_template_entities_tenant_active");
    }
}
