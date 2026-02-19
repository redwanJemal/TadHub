using TadHub.SharedKernel.Entities;

namespace Tenancy.Core.Entities;

/// <summary>
/// Represents a bilateral worker-sharing agreement between two agencies.
/// Allows one agency to share workers with another agency's pool.
/// </summary>
public class SharedPoolAgreement : BaseEntity
{
    /// <summary>
    /// The tenant providing workers to the pool.
    /// </summary>
    public Guid FromTenantId { get; set; }

    /// <summary>
    /// Navigation property for the providing tenant.
    /// </summary>
    public Tenant? FromTenant { get; set; }

    /// <summary>
    /// The tenant receiving access to the shared workers.
    /// </summary>
    public Guid ToTenantId { get; set; }

    /// <summary>
    /// Navigation property for the receiving tenant.
    /// </summary>
    public Tenant? ToTenant { get; set; }

    /// <summary>
    /// Current status of the agreement.
    /// </summary>
    public SharedPoolStatus Status { get; set; } = SharedPoolStatus.Pending;

    /// <summary>
    /// Date the agreement was approved.
    /// </summary>
    public DateTimeOffset? ApprovedAt { get; set; }

    /// <summary>
    /// Date the agreement was revoked (if revoked).
    /// </summary>
    public DateTimeOffset? RevokedAt { get; set; }

    /// <summary>
    /// Revenue split percentage for the providing agency (0-100).
    /// </summary>
    public decimal RevenueSplitPercentage { get; set; }

    /// <summary>
    /// URL to the signed agreement document.
    /// </summary>
    public string? AgreementDocumentUrl { get; set; }

    /// <summary>
    /// Additional terms or notes.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Workers shared under this agreement.
    /// </summary>
    public ICollection<SharedPoolWorker> SharedWorkers { get; set; } = new List<SharedPoolWorker>();

    /// <summary>
    /// Validates that the agreement is between two different tenants.
    /// </summary>
    public bool IsValid => FromTenantId != ToTenantId && FromTenantId != Guid.Empty && ToTenantId != Guid.Empty;
}
