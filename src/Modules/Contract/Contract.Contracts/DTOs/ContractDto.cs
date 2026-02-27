namespace Contract.Contracts.DTOs;

public sealed record ContractDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string ContractCode { get; init; } = string.Empty;

    // Type & Status
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset? StatusChangedAt { get; init; }
    public string? StatusReason { get; init; }

    // Parties (cross-module references, no FK)
    public Guid WorkerId { get; init; }
    public Guid ClientId { get; init; }
    public ContractWorkerDto? Worker { get; init; }
    public ContractClientDto? Client { get; init; }

    // Dates
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public DateOnly? ProbationEndDate { get; init; }
    public DateOnly? GuaranteeEndDate { get; init; }
    public bool ProbationPassed { get; init; }

    // Financial
    public decimal Rate { get; init; }
    public string RatePeriod { get; init; } = string.Empty;
    public string Currency { get; init; } = "AED";
    public decimal? TotalValue { get; init; }

    // Termination
    public DateTimeOffset? TerminatedAt { get; init; }
    public string? TerminationReason { get; init; }
    public string? TerminatedBy { get; init; }

    // Replacement linkage
    public Guid? ReplacementContractId { get; init; }
    public Guid? OriginalContractId { get; init; }

    public string? Notes { get; init; }

    // Audit
    public Guid? CreatedBy { get; init; }
    public Guid? UpdatedBy { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    // Optional
    public List<ContractStatusHistoryDto>? StatusHistory { get; init; }
}

public sealed record ContractWorkerDto
{
    public Guid Id { get; init; }
    public string FullNameEn { get; init; } = string.Empty;
    public string? FullNameAr { get; init; }
    public string WorkerCode { get; init; } = string.Empty;
}

public sealed record ContractClientDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string? NameAr { get; init; }
}
