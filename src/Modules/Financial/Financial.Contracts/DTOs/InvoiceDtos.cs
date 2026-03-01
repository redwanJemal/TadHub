using System.ComponentModel.DataAnnotations;

namespace Financial.Contracts.DTOs;

public sealed record InvoiceDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset? StatusChangedAt { get; init; }
    public Guid ContractId { get; init; }
    public Guid ClientId { get; init; }
    public Guid? WorkerId { get; init; }
    public InvoiceClientRef? Client { get; init; }
    public InvoiceWorkerRef? Worker { get; init; }
    public InvoiceContractRef? Contract { get; init; }
    public DateOnly IssueDate { get; init; }
    public DateOnly DueDate { get; init; }
    public decimal Subtotal { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal TaxableAmount { get; init; }
    public decimal VatRate { get; init; }
    public decimal VatAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal BalanceDue { get; init; }
    public string Currency { get; init; } = "AED";
    public string? TenantTrn { get; init; }
    public string? ClientTrn { get; init; }
    public Guid? DiscountProgramId { get; init; }
    public string? DiscountProgramName { get; init; }
    public string? DiscountCardNumber { get; init; }
    public decimal? DiscountPercentage { get; init; }
    public string? MilestoneType { get; init; }
    public Guid? OriginalInvoiceId { get; init; }
    public string? CreditNoteReason { get; init; }
    public string? Notes { get; init; }
    public Guid? CreatedBy { get; init; }
    public Guid? UpdatedBy { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public List<InvoiceLineItemDto>? LineItems { get; init; }
    public List<PaymentListDto>? Payments { get; init; }
}

public sealed record InvoiceListDto
{
    public Guid Id { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public Guid ContractId { get; init; }
    public Guid ClientId { get; init; }
    public Guid? WorkerId { get; init; }
    public InvoiceClientRef? Client { get; init; }
    public InvoiceWorkerRef? Worker { get; init; }
    public InvoiceContractRef? Contract { get; init; }
    public DateOnly IssueDate { get; init; }
    public DateOnly DueDate { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal PaidAmount { get; init; }
    public decimal BalanceDue { get; init; }
    public string Currency { get; init; } = "AED";
    public string? MilestoneType { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record InvoiceLineItemDto
{
    public Guid Id { get; init; }
    public int LineNumber { get; init; }
    public string Description { get; init; } = string.Empty;
    public string? DescriptionAr { get; init; }
    public decimal Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal LineTotal { get; init; }
    public string? ItemCode { get; init; }
}

public sealed record InvoiceSummaryDto
{
    public int TotalInvoices { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal TotalPaid { get; init; }
    public decimal TotalOutstanding { get; init; }
    public int OverdueCount { get; init; }
    public decimal OverdueAmount { get; init; }
    public Dictionary<string, int> CountsByStatus { get; init; } = new();
}

public sealed record CreateInvoiceRequest
{
    [Required] public Guid ContractId { get; init; }
    [Required] public Guid ClientId { get; init; }
    public Guid? WorkerId { get; init; }
    public string? Type { get; init; }
    [Required] public DateOnly IssueDate { get; init; }
    [Required] public DateOnly DueDate { get; init; }
    public string? MilestoneType { get; init; }
    public string Currency { get; init; } = "AED";
    public string? TenantTrn { get; init; }
    public string? ClientTrn { get; init; }
    public string? Notes { get; init; }
    public List<CreateInvoiceLineItemRequest> LineItems { get; init; } = [];
}

public sealed record CreateInvoiceLineItemRequest
{
    [Required] [MaxLength(500)] public string Description { get; init; } = string.Empty;
    [MaxLength(500)] public string? DescriptionAr { get; init; }
    [Required] [Range(0.01, double.MaxValue)] public decimal Quantity { get; init; }
    [Required] [Range(0, double.MaxValue)] public decimal UnitPrice { get; init; }
    public decimal DiscountAmount { get; init; }
    [MaxLength(50)] public string? ItemCode { get; init; }
}

public sealed record UpdateInvoiceRequest
{
    public DateOnly? IssueDate { get; init; }
    public DateOnly? DueDate { get; init; }
    public string? TenantTrn { get; init; }
    public string? ClientTrn { get; init; }
    public string? Notes { get; init; }
    public List<CreateInvoiceLineItemRequest>? LineItems { get; init; }
}

public sealed record TransitionInvoiceStatusRequest
{
    [Required] public string Status { get; init; } = string.Empty;
    public string? Reason { get; init; }
}

public sealed record GenerateInvoiceRequest
{
    [Required] public Guid ContractId { get; init; }
    public string? MilestoneType { get; init; }
    public decimal? OverrideAmount { get; init; }
    public string? Notes { get; init; }
}

public sealed record CreateCreditNoteRequest
{
    [Required] [MaxLength(500)] public string Reason { get; init; } = string.Empty;
    public decimal? Amount { get; init; }
    public string? Notes { get; init; }
}

public sealed record ApplyDiscountRequest
{
    [Required] public Guid DiscountProgramId { get; init; }
    [MaxLength(100)] public string? CardNumber { get; init; }
}

// Nested ref DTOs for BFF enrichment
public sealed record InvoiceClientRef
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string? NameAr { get; init; }
}

public sealed record InvoiceWorkerRef
{
    public Guid Id { get; init; }
    public string FullNameEn { get; init; } = string.Empty;
    public string? FullNameAr { get; init; }
    public string WorkerCode { get; init; } = string.Empty;
}

public sealed record InvoiceContractRef
{
    public Guid Id { get; init; }
    public string ContractCode { get; init; } = string.Empty;
}

public sealed record InvoiceRef
{
    public Guid Id { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
}
