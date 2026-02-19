using TadHub.SharedKernel.Entities;

namespace Tenancy.Core.Entities;

/// <summary>
/// Tracks which workers are shared in a pool agreement.
/// This is an audit trail - workers can be added and removed.
/// </summary>
public class SharedPoolWorker : BaseEntity
{
    /// <summary>
    /// The shared pool agreement this worker belongs to.
    /// </summary>
    public Guid SharedPoolAgreementId { get; set; }

    /// <summary>
    /// Navigation property for the agreement.
    /// </summary>
    public SharedPoolAgreement? SharedPoolAgreement { get; set; }

    /// <summary>
    /// The worker being shared (references Worker module).
    /// </summary>
    public Guid WorkerId { get; set; }

    /// <summary>
    /// Date the worker was added to the pool.
    /// </summary>
    public DateTimeOffset SharedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Date the worker was removed from the pool (null if still active).
    /// </summary>
    public DateTimeOffset? RevokedAt { get; set; }

    /// <summary>
    /// Whether the worker is currently in the pool.
    /// </summary>
    public bool IsActive => RevokedAt == null;

    /// <summary>
    /// Notes about this sharing (e.g., reason for revocation).
    /// </summary>
    public string? Notes { get; set; }
}
