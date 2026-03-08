using TadHub.SharedKernel.Entities;

namespace Returnee.Core.Entities;

public class ReturneeCaseStatusHistory : TenantScopedEntity
{
    public Guid ReturneeCaseId { get; set; }
    public ReturneeCaseStatus? FromStatus { get; set; }
    public ReturneeCaseStatus ToStatus { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
    public string? ChangedBy { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public ReturneeCase ReturneeCase { get; set; } = null!;
}
