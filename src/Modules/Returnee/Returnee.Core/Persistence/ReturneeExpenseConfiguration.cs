using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Returnee.Core.Entities;

namespace Returnee.Core.Persistence;

public class ReturneeExpenseConfiguration : IEntityTypeConfiguration<ReturneeExpense>
{
    public void Configure(EntityTypeBuilder<ReturneeExpense> builder)
    {
        builder.ToTable("returnee_expenses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ExpenseType)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.Currency)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.PaidBy)
            .IsRequired()
            .HasMaxLength(30)
            .HasConversion<string>();

        // Indexes
        builder.HasIndex(x => x.ReturneeCaseId)
            .HasDatabaseName("ix_returnee_expenses_case");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_returnee_expenses_tenant");
    }
}
