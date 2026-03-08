using TadHub.SharedKernel.Entities;

namespace Runaway.Core.Entities;

public class RunawayExpense : TenantScopedEntity
{
    public Guid RunawayCaseId { get; set; }
    public RunawayExpenseType ExpenseType { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AED";
    public string? Description { get; set; }
    public PaidByParty PaidBy { get; set; }

    // Navigation
    public RunawayCase RunawayCase { get; set; } = null!;
}
