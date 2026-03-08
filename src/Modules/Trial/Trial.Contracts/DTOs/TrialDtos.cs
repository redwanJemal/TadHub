using System.ComponentModel.DataAnnotations;

namespace Trial.Contracts.DTOs;

public sealed record TrialDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string TrialCode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset StatusChangedAt { get; init; }

    // Cross-module refs
    public Guid WorkerId { get; init; }
    public Guid ClientId { get; init; }
    public Guid? PlacementId { get; init; }
    public Guid? ContractId { get; init; }

    // BFF-enriched
    public TrialWorkerRefDto? Worker { get; init; }
    public TrialClientRefDto? Client { get; init; }

    // Trial period
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public int DaysRemaining { get; init; }

    // Outcome
    public string? Outcome { get; init; }
    public string? OutcomeNotes { get; init; }
    public DateTimeOffset? OutcomeDate { get; init; }

    // Audit
    public Guid? CreatedBy { get; init; }
    public Guid? UpdatedBy { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    // Optional includes
    public List<TrialStatusHistoryDto>? StatusHistory { get; init; }
}

public sealed record TrialListDto
{
    public Guid Id { get; init; }
    public string TrialCode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public Guid WorkerId { get; init; }
    public Guid ClientId { get; init; }
    public Guid? PlacementId { get; init; }
    public Guid? ContractId { get; init; }
    public TrialWorkerRefDto? Worker { get; init; }
    public TrialClientRefDto? Client { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public int DaysRemaining { get; init; }
    public string? Outcome { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record TrialWorkerRefDto
{
    public Guid Id { get; init; }
    public string FullNameEn { get; init; } = string.Empty;
    public string? FullNameAr { get; init; }
    public string WorkerCode { get; init; } = string.Empty;
}

public sealed record TrialClientRefDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string? NameAr { get; init; }
}

public sealed record TrialStatusHistoryDto
{
    public Guid Id { get; init; }
    public Guid TrialId { get; init; }
    public string? FromStatus { get; init; }
    public string ToStatus { get; init; } = string.Empty;
    public DateTimeOffset ChangedAt { get; init; }
    public string? ChangedBy { get; init; }
    public string? Reason { get; init; }
    public string? Notes { get; init; }
}

public sealed record CreateTrialRequest
{
    [Required]
    public Guid WorkerId { get; init; }

    [Required]
    public Guid ClientId { get; init; }

    [Required]
    public DateOnly StartDate { get; init; }

    public Guid? PlacementId { get; init; }

    [MaxLength(2000)]
    public string? Notes { get; init; }
}

public sealed record CompleteTrialRequest
{
    [Required]
    public string Outcome { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? OutcomeNotes { get; init; }
}

public sealed record CancelTrialRequest
{
    [MaxLength(500)]
    public string? Reason { get; init; }
}
