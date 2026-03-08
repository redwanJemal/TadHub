using TadHub.SharedKernel.Entities;

namespace Returnee.Core.Entities;

public class ReturneeExpense : TenantScopedEntity
{
    public Guid ReturneeCaseId { get; set; }
    public ExpenseType ExpenseType { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AED";
    public string? Description { get; set; }
    public PaidByParty PaidBy { get; set; }

    // Navigation
    public ReturneeCase ReturneeCase { get; set; } = null!;
}
