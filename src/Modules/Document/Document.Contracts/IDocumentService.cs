using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Document.Contracts;

// ──── DTOs ────

public sealed record WorkerDocumentDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public Guid WorkerId { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string? DocumentNumber { get; init; }
    public DateOnly? IssuedAt { get; init; }
    public DateOnly? ExpiresAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public string EffectiveStatus { get; init; } = string.Empty;
    public int? DaysUntilExpiry { get; init; }
    public string? IssuingAuthority { get; init; }
    public string? Notes { get; init; }
    public string? FileUrl { get; init; }
    public Guid? CreatedBy { get; init; }
    public Guid? UpdatedBy { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record WorkerDocumentListDto
{
    public Guid Id { get; init; }
    public Guid WorkerId { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string? DocumentNumber { get; init; }
    public DateOnly? IssuedAt { get; init; }
    public DateOnly? ExpiresAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public string EffectiveStatus { get; init; } = string.Empty;
    public int? DaysUntilExpiry { get; init; }
    public bool HasFile { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    // Enriched at API layer
    public string? WorkerName { get; init; }
    public string? WorkerCode { get; init; }
}

public sealed record CreateWorkerDocumentRequest
{
    public Guid WorkerId { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string? DocumentNumber { get; init; }
    public DateOnly? IssuedAt { get; init; }
    public DateOnly? ExpiresAt { get; init; }
    public string? Status { get; init; }
    public string? IssuingAuthority { get; init; }
    public string? Notes { get; init; }
}

public sealed record UpdateWorkerDocumentRequest
{
    public string? DocumentNumber { get; init; }
    public DateOnly? IssuedAt { get; init; }
    public DateOnly? ExpiresAt { get; init; }
    public string? Status { get; init; }
    public string? IssuingAuthority { get; init; }
    public string? Notes { get; init; }
}

public sealed record ComplianceSummaryDto
{
    public int TotalDocuments { get; init; }
    public int Valid { get; init; }
    public int ExpiringSoon { get; init; }
    public int Expired { get; init; }
    public int Pending { get; init; }
    public List<ComplianceByTypeDto> ByType { get; init; } = [];
}

public sealed record ComplianceByTypeDto
{
    public string DocumentType { get; init; } = string.Empty;
    public int Valid { get; init; }
    public int ExpiringSoon { get; init; }
    public int Expired { get; init; }
    public int Pending { get; init; }
}

// ──── Service interface ────

public interface IDocumentService
{
    Task<PagedList<WorkerDocumentListDto>> ListByWorkerAsync(
        Guid tenantId, Guid workerId, QueryParameters qp, CancellationToken ct = default);

    Task<PagedList<WorkerDocumentListDto>> ListAllAsync(
        Guid tenantId, QueryParameters qp, CancellationToken ct = default);

    Task<Result<WorkerDocumentDto>> GetByIdAsync(
        Guid tenantId, Guid id, CancellationToken ct = default);

    Task<Result<WorkerDocumentDto>> CreateAsync(
        Guid tenantId, CreateWorkerDocumentRequest request, CancellationToken ct = default);

    Task<Result<WorkerDocumentDto>> UpdateAsync(
        Guid tenantId, Guid id, UpdateWorkerDocumentRequest request, CancellationToken ct = default);

    Task<Result> DeleteAsync(
        Guid tenantId, Guid id, CancellationToken ct = default);

    Task<Result> SetFileUrlAsync(
        Guid tenantId, Guid id, string fileUrl, CancellationToken ct = default);

    Task<PagedList<WorkerDocumentListDto>> GetExpiringDocumentsAsync(
        Guid tenantId, int withinDays, QueryParameters qp, CancellationToken ct = default);

    Task<ComplianceSummaryDto> GetComplianceSummaryAsync(
        Guid tenantId, CancellationToken ct = default);
}
