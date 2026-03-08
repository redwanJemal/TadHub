using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Runaway.Core.Entities;

namespace Runaway.Core.Persistence;

public class RunawayExpenseConfiguration : IEntityTypeConfiguration<RunawayExpense>
{
    public void Configure(EntityTypeBuilder<RunawayExpense> builder)
    {
        builder.ToTable("runaway_expenses");

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
        builder.HasIndex(x => x.RunawayCaseId)
            .HasDatabaseName("ix_runaway_expenses_case");

        builder.HasIndex(x => x.TenantId)
            .HasDatabaseName("ix_runaway_expenses_tenant");
    }
}
