using TadHub.SharedKernel.Entities;

namespace Worker.Core.Entities;

/// <summary>
/// Worker state transition history (append-only audit trail).
/// Every state change creates a new record.
/// </summary>
public class WorkerStateHistory : TenantScopedEntity
{
    /// <summary>
    /// Worker ID FK.
    /// </summary>
    public Guid WorkerId { get; set; }

    /// <summary>
    /// Worker navigation.
    /// </summary>
    public Worker? Worker { get; set; }

    /// <summary>
    /// Previous status.
    /// </summary>
    public WorkerStatus FromStatus { get; set; }

    /// <summary>
    /// New status.
    /// </summary>
    public WorkerStatus ToStatus { get; set; }

    /// <summary>
    /// Reason for the transition.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// User who triggered the transition.
    /// </summary>
    public Guid TriggeredByUserId { get; set; }

    /// <summary>
    /// Related entity ID (contractId, medicalReportId, etc.).
    /// </summary>
    public Guid? RelatedEntityId { get; set; }

    /// <summary>
    /// When the transition occurred.
    /// </summary>
    public DateTimeOffset OccurredAt { get; set; }
}
