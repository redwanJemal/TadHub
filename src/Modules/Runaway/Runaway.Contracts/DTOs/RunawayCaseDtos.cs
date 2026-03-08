using System.ComponentModel.DataAnnotations;

namespace Runaway.Contracts.DTOs;

public sealed record RunawayCaseDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string CaseCode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset StatusChangedAt { get; init; }

    // Cross-module refs
    public Guid WorkerId { get; init; }
    public Guid ContractId { get; init; }
    public Guid ClientId { get; init; }
    public Guid? SupplierId { get; init; }

    // BFF-enriched
    public RunawayWorkerRefDto? Worker { get; init; }
    public RunawayClientRefDto? Client { get; init; }

    // Case info
    public DateTimeOffset ReportedDate { get; init; }
    public string ReportedBy { get; init; } = string.Empty;
    public string? LastKnownLocation { get; init; }
    public string? PoliceReportNumber { get; init; }
    public DateTimeOffset? PoliceReportDate { get; init; }

    // Guarantee
    public bool IsWithinGuarantee { get; init; }
    public string? GuaranteePeriodType { get; init; }

    public string? Notes { get; init; }

    // Timestamps
    public DateTimeOffset? ConfirmedAt { get; init; }
    public DateTimeOffset? SettledAt { get; init; }
    public DateTimeOffset? ClosedAt { get; init; }

    // Audit
    public Guid? CreatedBy { get; init; }
    public Guid? UpdatedBy { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    // Optional includes
    public List<RunawayExpenseDto>? Expenses { get; init; }
    public List<RunawayCaseStatusHistoryDto>? StatusHistory { get; init; }
}

public sealed record RunawayCaseListDto
{
    public Guid Id { get; init; }
    public string CaseCode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public Guid WorkerId { get; init; }
    public Guid ContractId { get; init; }
    public Guid ClientId { get; init; }
    public Guid? SupplierId { get; init; }
    public RunawayWorkerRefDto? Worker { get; init; }
    public RunawayClientRefDto? Client { get; init; }
    public DateTimeOffset ReportedDate { get; init; }
    public bool IsWithinGuarantee { get; init; }
    public string? PoliceReportNumber { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record RunawayWorkerRefDto
{
    public Guid Id { get; init; }
    public string FullNameEn { get; init; } = string.Empty;
    public string? FullNameAr { get; init; }
    public string WorkerCode { get; init; } = string.Empty;
}

public sealed record RunawayClientRefDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string? NameAr { get; init; }
}

public sealed record RunawayExpenseDto
{
    public Guid Id { get; init; }
    public Guid RunawayCaseId { get; init; }
    public string ExpenseType { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "AED";
    public string? Description { get; init; }
    public string PaidBy { get; init; } = string.Empty;
}

public sealed record RunawayCaseStatusHistoryDto
{
    public Guid Id { get; init; }
    public Guid RunawayCaseId { get; init; }
    public string? FromStatus { get; init; }
    public string ToStatus { get; init; } = string.Empty;
    public DateTimeOffset ChangedAt { get; init; }
    public string? ChangedBy { get; init; }
    public string? Reason { get; init; }
    public string? Notes { get; init; }
}

public sealed record ReportRunawayCaseRequest
{
    [Required]
    public Guid WorkerId { get; init; }

    [Required]
    public Guid ContractId { get; init; }

    [Required]
    public Guid ClientId { get; init; }

    public Guid? SupplierId { get; init; }

    [Required]
    public DateTimeOffset ReportedDate { get; init; }

    [Required]
    [MaxLength(200)]
    public string ReportedBy { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? LastKnownLocation { get; init; }

    [MaxLength(100)]
    public string? PoliceReportNumber { get; init; }

    public DateTimeOffset? PoliceReportDate { get; init; }

    [MaxLength(2000)]
    public string? Notes { get; init; }
}

public sealed record UpdateRunawayCaseRequest
{
    [MaxLength(500)]
    public string? LastKnownLocation { get; init; }

    [MaxLength(100)]
    public string? PoliceReportNumber { get; init; }

    public DateTimeOffset? PoliceReportDate { get; init; }

    [MaxLength(2000)]
    public string? Notes { get; init; }
}

public sealed record ConfirmRunawayCaseRequest
{
    [MaxLength(2000)]
    public string? Notes { get; init; }
}

public sealed record SettleRunawayCaseRequest
{
    [MaxLength(2000)]
    public string? Notes { get; init; }
}

public sealed record CloseRunawayCaseRequest
{
    [MaxLength(2000)]
    public string? Notes { get; init; }
}

public sealed record CreateRunawayExpenseRequest
{
    [Required]
    [MaxLength(30)]
    public string ExpenseType { get; init; } = string.Empty;

    [Required]
    public decimal Amount { get; init; }

    [MaxLength(500)]
    public string? Description { get; init; }

    [Required]
    [MaxLength(30)]
    public string PaidBy { get; init; } = string.Empty;
}
