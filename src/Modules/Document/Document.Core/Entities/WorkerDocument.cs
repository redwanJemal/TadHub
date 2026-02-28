using TadHub.SharedKernel.Entities;

namespace Document.Core.Entities;

public class WorkerDocument : SoftDeletableEntity, IAuditable
{
    public Guid WorkerId { get; set; }

    public DocumentType DocumentType { get; set; }

    public string? DocumentNumber { get; set; }

    public DateOnly? IssuedAt { get; set; }

    public DateOnly? ExpiresAt { get; set; }

    public DocumentStatus Status { get; set; } = DocumentStatus.Pending;

    public string? IssuingAuthority { get; set; }

    public string? Notes { get; set; }

    public string? FileUrl { get; set; }

    // IAuditable
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Computed helpers
    public int? DaysUntilExpiry => ExpiresAt.HasValue
        ? (ExpiresAt.Value.ToDateTime(TimeOnly.MinValue) - DateTime.UtcNow.Date).Days
        : null;

    public bool IsExpired => ExpiresAt.HasValue
        && ExpiresAt.Value < DateOnly.FromDateTime(DateTime.UtcNow);

    public bool IsExpiringSoon(int thresholdDays = 30) =>
        ExpiresAt.HasValue && !IsExpired && DaysUntilExpiry <= thresholdDays;
}
