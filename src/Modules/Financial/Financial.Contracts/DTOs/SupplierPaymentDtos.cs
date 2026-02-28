using System.ComponentModel.DataAnnotations;

namespace Financial.Contracts.DTOs;

public sealed record SupplierPaymentDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string PaymentNumber { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public Guid SupplierId { get; init; }
    public Guid? WorkerId { get; init; }
    public Guid? ContractId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "AED";
    public string Method { get; init; } = string.Empty;
    public string? ReferenceNumber { get; init; }
    public DateOnly PaymentDate { get; init; }
    public DateTimeOffset? PaidAt { get; init; }
    public string? Notes { get; init; }
    public Guid? CreatedBy { get; init; }
    public Guid? UpdatedBy { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record SupplierPaymentListDto
{
    public Guid Id { get; init; }
    public string PaymentNumber { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public Guid SupplierId { get; init; }
    public Guid? WorkerId { get; init; }
    public Guid? ContractId { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "AED";
    public string Method { get; init; } = string.Empty;
    public DateOnly PaymentDate { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record CreateSupplierPaymentRequest
{
    [Required] public Guid SupplierId { get; init; }
    public Guid? WorkerId { get; init; }
    public Guid? ContractId { get; init; }
    [Required] [Range(0.01, double.MaxValue)] public decimal Amount { get; init; }
    public string Currency { get; init; } = "AED";
    [Required] public string Method { get; init; } = "BankTransfer";
    [MaxLength(100)] public string? ReferenceNumber { get; init; }
    [Required] public DateOnly PaymentDate { get; init; }
    [MaxLength(2000)] public string? Notes { get; init; }
}

public sealed record UpdateSupplierPaymentRequest
{
    [Range(0.01, double.MaxValue)] public decimal? Amount { get; init; }
    public string? Method { get; init; }
    [MaxLength(100)] public string? ReferenceNumber { get; init; }
    public DateOnly? PaymentDate { get; init; }
    [MaxLength(2000)] public string? Notes { get; init; }
}

public sealed record TransitionSupplierPaymentStatusRequest
{
    [Required] public string Status { get; init; } = string.Empty;
    public string? Reason { get; init; }
}
