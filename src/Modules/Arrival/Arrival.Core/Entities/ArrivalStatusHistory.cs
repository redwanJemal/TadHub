using TadHub.SharedKernel.Entities;

namespace Arrival.Core.Entities;

public class ArrivalStatusHistory : TenantScopedEntity
{
    public Guid ArrivalId { get; set; }
    public ArrivalStatus? FromStatus { get; set; }
    public ArrivalStatus ToStatus { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
    public string? ChangedBy { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Arrival Arrival { get; set; } = null!;
}
