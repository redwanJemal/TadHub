using TadHub.SharedKernel.Entities;

namespace Worker.Core.Entities;

public class WorkerStatusHistory : TenantScopedEntity
{
    public Guid WorkerId { get; set; }
    public WorkerStatus? FromStatus { get; set; }
    public WorkerStatus ToStatus { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
    public Guid? ChangedBy { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Worker Worker { get; set; } = null!;
}
