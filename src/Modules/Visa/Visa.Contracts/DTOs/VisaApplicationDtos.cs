namespace Visa.Contracts.DTOs;

public sealed record VisaApplicationDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string ApplicationCode { get; init; } = string.Empty;
    public string VisaType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset StatusChangedAt { get; init; }
    public string? StatusReason { get; init; }

    // Cross-module refs
    public Guid WorkerId { get; init; }
    public Guid ClientId { get; init; }
    public Guid? ContractId { get; init; }
    public Guid? PlacementId { get; init; }

    // BFF-enriched
    public VisaWorkerRefDto? Worker { get; init; }
    public VisaClientRefDto? Client { get; init; }

    // Dates
    public DateOnly? ApplicationDate { get; init; }
    public DateOnly? ApprovalDate { get; init; }
    public DateOnly? IssuanceDate { get; init; }
    public DateOnly? ExpiryDate { get; init; }

    // Reference info
    public string? ReferenceNumber { get; init; }
    public string? VisaNumber { get; init; }
    public string? Notes { get; init; }
    public string? RejectionReason { get; init; }

    // Audit
    public Guid? CreatedBy { get; init; }
    public Guid? UpdatedBy { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    // Optional includes
    public List<VisaApplicationStatusHistoryDto>? StatusHistory { get; init; }
    public List<VisaApplicationDocumentDto>? Documents { get; init; }
}

public sealed record VisaApplicationListDto
{
    public Guid Id { get; init; }
    public string ApplicationCode { get; init; } = string.Empty;
    public string VisaType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset StatusChangedAt { get; init; }
    public Guid WorkerId { get; init; }
    public Guid ClientId { get; init; }
    public Guid? PlacementId { get; init; }
    public VisaWorkerRefDto? Worker { get; init; }
    public VisaClientRefDto? Client { get; init; }
    public DateOnly? ApplicationDate { get; init; }
    public DateOnly? ExpiryDate { get; init; }
    public string? ReferenceNumber { get; init; }
    public int DocumentCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record VisaWorkerRefDto
{
    public Guid Id { get; init; }
    public string FullNameEn { get; init; } = string.Empty;
    public string? FullNameAr { get; init; }
    public string WorkerCode { get; init; } = string.Empty;
}

public sealed record VisaClientRefDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string? NameAr { get; init; }
}

public sealed record VisaApplicationStatusHistoryDto
{
    public Guid Id { get; init; }
    public Guid VisaApplicationId { get; init; }
    public string? FromStatus { get; init; }
    public string ToStatus { get; init; } = string.Empty;
    public DateTimeOffset ChangedAt { get; init; }
    public string? ChangedBy { get; init; }
    public string? Reason { get; init; }
    public string? Notes { get; init; }
}

public sealed record VisaApplicationDocumentDto
{
    public Guid Id { get; init; }
    public Guid VisaApplicationId { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string FileUrl { get; init; } = string.Empty;
    public DateTimeOffset UploadedAt { get; init; }
    public bool IsVerified { get; init; }
}

public sealed record CreateVisaApplicationRequest
{
    public Guid WorkerId { get; init; }
    public Guid ClientId { get; init; }
    public string VisaType { get; init; } = string.Empty;
    public Guid? ContractId { get; init; }
    public Guid? PlacementId { get; init; }
    public DateOnly? ApplicationDate { get; init; }
    public string? ReferenceNumber { get; init; }
    public string? Notes { get; init; }
}

public sealed record UpdateVisaApplicationRequest
{
    public DateOnly? ApplicationDate { get; init; }
    public DateOnly? ApprovalDate { get; init; }
    public DateOnly? IssuanceDate { get; init; }
    public DateOnly? ExpiryDate { get; init; }
    public string? ReferenceNumber { get; init; }
    public string? VisaNumber { get; init; }
    public string? Notes { get; init; }
    public Guid? ContractId { get; init; }
    public Guid? PlacementId { get; init; }
}

public sealed record TransitionVisaStatusRequest
{
    public string Status { get; init; } = string.Empty;
    public string? Reason { get; init; }
    public string? Notes { get; init; }
}

public sealed record UploadVisaDocumentRequest
{
    public string DocumentType { get; init; } = string.Empty;
    public string FileUrl { get; init; } = string.Empty;
}
