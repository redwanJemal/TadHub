using System.ComponentModel.DataAnnotations;

namespace Financial.Contracts.DTOs;

public sealed record SupplierDebitDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string DebitNumber { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public Guid SupplierId { get; init; }
    public Guid? WorkerId { get; init; }
    public Guid? ContractId { get; init; }
    public string? CaseType { get; init; }
    public Guid? CaseId { get; init; }
    public string DebitType { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "AED";
    public Guid? SettlementPaymentId { get; init; }
    public DateTimeOffset? SettledAt { get; init; }
    public string? Notes { get; init; }
    public Guid? CreatedBy { get; init; }
    public Guid? UpdatedBy { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record SupplierDebitListDto
{
    public Guid Id { get; init; }
    public string DebitNumber { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public Guid SupplierId { get; init; }
    public Guid? WorkerId { get; init; }
    public Guid? ContractId { get; init; }
    public string? CaseType { get; init; }
    public string DebitType { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "AED";
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record CreateSupplierDebitRequest
{
    [Required] public Guid SupplierId { get; init; }
    public Guid? WorkerId { get; init; }
    public Guid? ContractId { get; init; }
    public string? CaseType { get; init; }
    public Guid? CaseId { get; init; }
    [Required] public string DebitType { get; init; } = string.Empty;
    [Required] [MaxLength(500)] public string Description { get; init; } = string.Empty;
    [Required] [Range(0.01, double.MaxValue)] public decimal Amount { get; init; }
    public string Currency { get; init; } = "AED";
    [MaxLength(2000)] public string? Notes { get; init; }
}

public sealed record UpdateSupplierDebitRequest
{
    [MaxLength(500)] public string? Description { get; init; }
    [Range(0.01, double.MaxValue)] public decimal? Amount { get; init; }
    [MaxLength(2000)] public string? Notes { get; init; }
}

public sealed record TransitionSupplierDebitStatusRequest
{
    [Required] public string Status { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public Guid? SettlementPaymentId { get; init; }
}
