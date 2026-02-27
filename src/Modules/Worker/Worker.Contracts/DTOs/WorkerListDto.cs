namespace Worker.Contracts.DTOs;

/// <summary>
/// Lightweight worker DTO for list views.
/// </summary>
public sealed record WorkerListDto
{
    public Guid Id { get; init; }
    public string WorkerCode { get; init; } = string.Empty;
    public string FullNameEn { get; init; } = string.Empty;
    public string? FullNameAr { get; init; }
    public string Nationality { get; init; } = string.Empty;
    public string? Gender { get; init; }
    public string SourceType { get; init; } = string.Empty;
    public Guid? TenantSupplierId { get; init; }
    public WorkerSupplierDto? Supplier { get; init; }
    public Guid? JobCategoryId { get; init; }
    public JobCategoryInfoDto? JobCategory { get; init; }
    public string Status { get; init; } = string.Empty;
    public string? PhotoUrl { get; init; }
    public DateTimeOffset? ActivatedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
