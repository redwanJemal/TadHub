using TadHub.SharedKernel.Entities;

namespace Worker.Core.Entities;

/// <summary>
/// Worker passport custody record (append-only audit trail).
/// Every passport transfer creates a new record.
/// </summary>
public class WorkerPassportCustody : TenantScopedEntity
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
    /// Custody location.
    /// </summary>
    public PassportLocation Location { get; set; }

    /// <summary>
    /// Name of person/entity receiving the passport.
    /// </summary>
    public string? HandedToName { get; set; }

    /// <summary>
    /// Entity ID (clientId, immigrationFileId, etc.).
    /// </summary>
    public Guid? HandedToEntityId { get; set; }

    /// <summary>
    /// When the passport was handed over.
    /// </summary>
    public DateTimeOffset? HandedAt { get; set; }

    /// <summary>
    /// When the passport was received at this location.
    /// </summary>
    public DateTimeOffset? ReceivedAt { get; set; }

    /// <summary>
    /// User who recorded this transfer.
    /// </summary>
    public Guid RecordedByUserId { get; set; }

    /// <summary>
    /// Notes about the transfer.
    /// </summary>
    public string? Notes { get; set; }
}
