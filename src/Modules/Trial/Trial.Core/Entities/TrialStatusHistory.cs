using TadHub.SharedKernel.Entities;

namespace Trial.Core.Entities;

public class TrialStatusHistory : TenantScopedEntity
{
    public Guid TrialId { get; set; }
    public TrialStatus? FromStatus { get; set; }
    public TrialStatus ToStatus { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
    public string? ChangedBy { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Trial Trial { get; set; } = null!;
}
