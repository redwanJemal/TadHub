using Supplier.Core.Entities;
using TadHub.SharedKernel.Entities;

namespace Candidate.Core.Entities;

/// <summary>
/// Represents a person submitted to a Tadbeer center for potential recruitment.
/// Candidates flow through a status pipeline before being converted to active workers.
/// </summary>
public class Candidate : SoftDeletableEntity, IAuditable
{
    // Personal
    public string FullNameEn { get; set; } = string.Empty;
    public string? FullNameAr { get; set; }
    public string Nationality { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? PassportNumber { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    // Sourcing
    public CandidateSourceType SourceType { get; set; }
    public Guid? TenantSupplierId { get; set; }

    // Pipeline
    public CandidateStatus Status { get; set; } = CandidateStatus.Received;
    public DateTimeOffset? StatusChangedAt { get; set; }
    public string? StatusReason { get; set; }

    // Document tracking
    public DateOnly? PassportExpiry { get; set; }
    public string? MedicalStatus { get; set; }
    public string? VisaStatus { get; set; }

    // Operational
    public DateOnly? ExpectedArrivalDate { get; set; }
    public DateOnly? ActualArrivalDate { get; set; }
    public string? Notes { get; set; }
    public string? ExternalReference { get; set; }

    // IAuditable
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation
    public TenantSupplier? TenantSupplier { get; set; }
    public ICollection<CandidateStatusHistory> StatusHistory { get; set; } = new List<CandidateStatusHistory>();
}
