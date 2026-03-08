using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Visa.Core.Entities;

namespace Visa.Core.Persistence;

public class VisaApplicationDocumentConfiguration : IEntityTypeConfiguration<VisaApplicationDocument>
{
    public void Configure(EntityTypeBuilder<VisaApplicationDocument> builder)
    {
        builder.ToTable("visa_application_documents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DocumentType)
            .IsRequired()
            .HasMaxLength(50)
            .HasConversion<string>();

        builder.Property(x => x.FileUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(x => x.VisaApplicationId)
            .HasDatabaseName("ix_visa_application_documents_application");
    }
}
