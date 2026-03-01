using System.ComponentModel.DataAnnotations;

namespace Financial.Contracts.DTOs;

public sealed record PaymentDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string PaymentNumber { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public Guid InvoiceId { get; init; }
    public Guid ClientId { get; init; }
    public InvoiceRef? Invoice { get; init; }
    public InvoiceClientRef? Client { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "AED";
    public string Method { get; init; } = string.Empty;
    public string? ReferenceNumber { get; init; }
    public DateOnly PaymentDate { get; init; }
    public string? GatewayProvider { get; init; }
    public string? GatewayTransactionId { get; init; }
    public string? GatewayStatus { get; init; }
    public Guid? RefundedPaymentId { get; init; }
    public decimal? RefundAmount { get; init; }
    public Guid? CashierId { get; init; }
    public string? CashierName { get; init; }
    public string? Notes { get; init; }
    public Guid? CreatedBy { get; init; }
    public Guid? UpdatedBy { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record PaymentListDto
{
    public Guid Id { get; init; }
    public string PaymentNumber { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public Guid InvoiceId { get; init; }
    public Guid ClientId { get; init; }
    public InvoiceRef? Invoice { get; init; }
    public InvoiceClientRef? Client { get; init; }
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "AED";
    public string Method { get; init; } = string.Empty;
    public string? ReferenceNumber { get; init; }
    public DateOnly PaymentDate { get; init; }
    public string? CashierName { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record RecordPaymentRequest
{
    [Required] public Guid InvoiceId { get; init; }
    [Required] public Guid ClientId { get; init; }
    [Required] [Range(0.01, double.MaxValue)] public decimal Amount { get; init; }
    public string Currency { get; init; } = "AED";
    [Required] public string Method { get; init; } = "Cash";
    [MaxLength(100)] public string? ReferenceNumber { get; init; }
    [Required] public DateOnly PaymentDate { get; init; }
    public string? GatewayProvider { get; init; }
    public Guid? CashierId { get; init; }
    [MaxLength(200)] public string? CashierName { get; init; }
    [MaxLength(2000)] public string? Notes { get; init; }
}

public sealed record TransitionPaymentStatusRequest
{
    [Required] public string Status { get; init; } = string.Empty;
    public string? Reason { get; init; }
}

public sealed record RefundPaymentRequest
{
    [Required] [Range(0.01, double.MaxValue)] public decimal Amount { get; init; }
    [Required] [MaxLength(500)] public string Reason { get; init; } = string.Empty;
    [MaxLength(2000)] public string? Notes { get; init; }
}

// Gateway abstraction DTOs
public sealed record PaymentGatewayRequest
{
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "AED";
    public string InvoiceNumber { get; init; } = string.Empty;
    public string? Description { get; init; }
    public Dictionary<string, string>? Metadata { get; init; }
}

public sealed record PaymentGatewayResult
{
    public bool Success { get; init; }
    public string? TransactionId { get; init; }
    public string? Status { get; init; }
    public string? RedirectUrl { get; init; }
    public string? Error { get; init; }
    public string? RawResponse { get; init; }
}
