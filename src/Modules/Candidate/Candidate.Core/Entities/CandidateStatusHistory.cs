using TadHub.SharedKernel.Entities;

namespace Candidate.Core.Entities;

/// <summary>
/// Audit trail entry for candidate status transitions.
/// </summary>
public class CandidateStatusHistory : TenantScopedEntity
{
    public Guid CandidateId { get; set; }
    public CandidateStatus? FromStatus { get; set; }
    public CandidateStatus ToStatus { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
    public Guid? ChangedBy { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public Candidate Candidate { get; set; } = null!;
}
