using TadHub.SharedKernel.Entities;

namespace Placement.Core.Entities;

public class PlacementCostItem : TenantScopedEntity
{
    public Guid PlacementId { get; set; }
    public PlacementCostType CostType { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "AED";
    public PlacementCostStatus Status { get; set; } = PlacementCostStatus.Pending;
    public DateOnly? CostDate { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Placement Placement { get; set; } = null!;
}
