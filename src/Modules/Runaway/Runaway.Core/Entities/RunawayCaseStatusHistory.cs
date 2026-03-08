using TadHub.SharedKernel.Entities;

namespace Runaway.Core.Entities;

public class RunawayCaseStatusHistory : TenantScopedEntity
{
    public Guid RunawayCaseId { get; set; }
    public RunawayCaseStatus? FromStatus { get; set; }
    public RunawayCaseStatus ToStatus { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
    public string? ChangedBy { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public RunawayCase RunawayCase { get; set; } = null!;
}
