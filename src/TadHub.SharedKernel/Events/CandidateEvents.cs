namespace TadHub.SharedKernel.Events;

/// <summary>
/// Snapshot of a candidate's skill at conversion time.
/// </summary>
public sealed record CandidateSnapshotSkill
{
    public string SkillName { get; init; } = string.Empty;
    public string ProficiencyLevel { get; init; } = string.Empty;
}

/// <summary>
/// Snapshot of a candidate's language at conversion time.
/// </summary>
public sealed record CandidateSnapshotLanguage
{
    public string Language { get; init; } = string.Empty;
    public string ProficiencyLevel { get; init; } = string.Empty;
}

/// <summary>
/// Snapshot of all candidate fields needed to create a Worker record.
/// </summary>
public sealed record CandidateSnapshotDto
{
    // Personal
    public string FullNameEn { get; init; } = string.Empty;
    public string? FullNameAr { get; init; }
    public string Nationality { get; init; } = string.Empty;
    public DateOnly? DateOfBirth { get; init; }
    public string? Gender { get; init; }
    public string? PassportNumber { get; init; }
    public DateOnly? PassportExpiry { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }

    // Professional
    public string? Religion { get; init; }
    public string? MaritalStatus { get; init; }
    public string? EducationLevel { get; init; }
    public Guid? JobCategoryId { get; init; }
    public int? ExperienceYears { get; init; }
    public decimal? MonthlySalary { get; init; }

    // Media
    public string? PhotoUrl { get; init; }
    public string? VideoUrl { get; init; }
    public string? PassportDocumentUrl { get; init; }

    // Source
    public string SourceType { get; init; } = string.Empty;
    public Guid? TenantSupplierId { get; init; }

    // Collections
    public List<CandidateSnapshotSkill> Skills { get; init; } = new();
    public List<CandidateSnapshotLanguage> Languages { get; init; } = new();
}

/// <summary>
/// Raised when a candidate is approved.
/// The Worker module consumes this to auto-create a Worker record.
/// </summary>
public sealed record CandidateApprovedEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; init; }
    public Guid TenantId { get; init; }
    public Guid CandidateId { get; init; }
    public CandidateSnapshotDto CandidateData { get; init; } = new();

    public CandidateApprovedEvent() { }
}
