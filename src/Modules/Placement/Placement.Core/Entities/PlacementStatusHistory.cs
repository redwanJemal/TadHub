using TadHub.SharedKernel.Entities;

namespace Placement.Core.Entities;

public class PlacementStatusHistory : TenantScopedEntity
{
    public Guid PlacementId { get; set; }
    public PlacementStatus? FromStatus { get; set; }
    public PlacementStatus ToStatus { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
    public string? ChangedBy { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Placement Placement { get; set; } = null!;
}
