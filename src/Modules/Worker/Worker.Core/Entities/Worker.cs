using TadHub.SharedKernel.Entities;

namespace Worker.Core.Entities;

/// <summary>
/// Represents an active worker in inventory.
/// Created automatically when a candidate is converted.
/// </summary>
public class Worker : SoftDeletableEntity, IAuditable
{
    // Reference
    public Guid CandidateId { get; set; }

    // Worker-specific
    public string WorkerCode { get; set; } = string.Empty;
    public WorkerStatus Status { get; set; } = WorkerStatus.Available;
    public WorkerLocation Location { get; set; } = WorkerLocation.Abroad;
    public DateTimeOffset? StatusChangedAt { get; set; }
    public string? StatusReason { get; set; }
    public DateTimeOffset? ActivatedAt { get; set; }
    public DateTimeOffset? TerminatedAt { get; set; }
    public string? TerminationReason { get; set; }
    public DateTimeOffset? ProcurementPaidAt { get; set; }
    public DateTimeOffset? FlightDate { get; set; }
    public DateTimeOffset? ArrivedAt { get; set; }
    public string? Notes { get; set; }

    // Personal (snapshot from candidate)
    public string FullNameEn { get; set; } = string.Empty;
    public string? FullNameAr { get; set; }
    public string Nationality { get; set; } = string.Empty;
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? PassportNumber { get; set; }
    public DateOnly? PassportExpiry { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    // Professional
    public string? Religion { get; set; }
    public string? MaritalStatus { get; set; }
    public string? EducationLevel { get; set; }
    public Guid? JobCategoryId { get; set; }
    public int? ExperienceYears { get; set; }
    public decimal? MonthlySalary { get; set; }

    // Media
    public string? PhotoUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? PassportDocumentUrl { get; set; }

    // Source
    public WorkerSourceType SourceType { get; set; }
    public Guid? TenantSupplierId { get; set; }

    // IAuditable
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation
    public ICollection<WorkerSkill> Skills { get; set; } = new List<WorkerSkill>();
    public ICollection<WorkerLanguage> Languages { get; set; } = new List<WorkerLanguage>();
    public ICollection<WorkerStatusHistory> StatusHistory { get; set; } = new List<WorkerStatusHistory>();
}
