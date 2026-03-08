using System.ComponentModel.DataAnnotations;

namespace Returnee.Contracts.DTOs;

public sealed record ReturneeCaseDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string CaseCode { get; init; } = string.Empty;
    public string ReturnType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset StatusChangedAt { get; init; }

    // Cross-module refs
    public Guid WorkerId { get; init; }
    public Guid ContractId { get; init; }
    public Guid ClientId { get; init; }
    public Guid? SupplierId { get; init; }

    // BFF-enriched
    public ReturneeWorkerRefDto? Worker { get; init; }
    public ReturneeClientRefDto? Client { get; init; }

    // Case info
    public DateOnly ReturnDate { get; init; }
    public string ReturnReason { get; init; } = string.Empty;
    public int MonthsWorked { get; init; }
    public bool IsWithinGuarantee { get; init; }
    public string? GuaranteePeriodType { get; init; }

    // Refund
    public decimal? TotalAmountPaid { get; init; }
    public decimal? RefundAmount { get; init; }
    public string Currency { get; init; } = "AED";

    // Approval / Rejection
    public string? ApprovedBy { get; init; }
    public DateTimeOffset? ApprovedAt { get; init; }
    public string? RejectedReason { get; init; }

    // Settlement
    public DateTimeOffset? SettledAt { get; init; }
    public string? SettlementNotes { get; init; }

    public string? Notes { get; init; }

    // Audit
    public Guid? CreatedBy { get; init; }
    public Guid? UpdatedBy { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    // Optional includes
    public List<ReturneeExpenseDto>? Expenses { get; init; }
    public List<ReturneeCaseStatusHistoryDto>? StatusHistory { get; init; }
}

public sealed record ReturneeCaseListDto
{
    public Guid Id { get; init; }
    public string CaseCode { get; init; } = string.Empty;
    public string ReturnType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public Guid WorkerId { get; init; }
    public Guid ContractId { get; init; }
    public Guid ClientId { get; init; }
    public ReturneeWorkerRefDto? Worker { get; init; }
    public ReturneeClientRefDto? Client { get; init; }
    public DateOnly ReturnDate { get; init; }
    public int MonthsWorked { get; init; }
    public bool IsWithinGuarantee { get; init; }
    public decimal? RefundAmount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record ReturneeWorkerRefDto
{
    public Guid Id { get; init; }
    public string FullNameEn { get; init; } = string.Empty;
    public string? FullNameAr { get; init; }
    public string WorkerCode { get; init; } = string.Empty;
}

public sealed record ReturneeClientRefDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string? NameAr { get; init; }
}

public sealed record ReturneeExpenseDto
{
    public Guid Id { get; init; }
    public Guid ReturneeCaseId { get; init; }
    public string ExpenseType { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "AED";
    public string? Description { get; init; }
    public string PaidBy { get; init; } = string.Empty;
}

public sealed record ReturneeCaseStatusHistoryDto
{
    public Guid Id { get; init; }
    public Guid ReturneeCaseId { get; init; }
    public string? FromStatus { get; init; }
    public string ToStatus { get; init; } = string.Empty;
    public DateTimeOffset ChangedAt { get; init; }
    public string? ChangedBy { get; init; }
    public string? Reason { get; init; }
    public string? Notes { get; init; }
}

public sealed record RefundCalculationDto
{
    public Guid ContractId { get; init; }
    public decimal TotalAmountPaid { get; init; }
    public int TotalContractMonths { get; init; }
    public int MonthsWorked { get; init; }
    public decimal ValuePerMonth { get; init; }
    public decimal RefundAmount { get; init; }
    public string Currency { get; init; } = "AED";
}

public sealed record CreateReturneeCaseRequest
{
    [Required]
    public Guid WorkerId { get; init; }

    [Required]
    public Guid ContractId { get; init; }

    [Required]
    public Guid ClientId { get; init; }

    public Guid? SupplierId { get; init; }

    [Required]
    [MaxLength(30)]
    public string ReturnType { get; init; } = string.Empty;

    [Required]
    public DateOnly ReturnDate { get; init; }

    [Required]
    [MaxLength(2000)]
    public string ReturnReason { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Notes { get; init; }
}

public sealed record ApproveReturneeCaseRequest
{
    [MaxLength(2000)]
    public string? Notes { get; init; }
}

public sealed record RejectReturneeCaseRequest
{
    [Required]
    [MaxLength(2000)]
    public string Reason { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Notes { get; init; }
}

public sealed record SettleReturneeCaseRequest
{
    [MaxLength(2000)]
    public string? SettlementNotes { get; init; }
}

public sealed record CreateReturneeExpenseRequest
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
