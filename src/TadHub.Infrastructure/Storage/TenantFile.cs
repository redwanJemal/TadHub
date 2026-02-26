using TadHub.SharedKernel.Entities;

namespace TadHub.Infrastructure.Storage;

/// <summary>
/// Tracks files uploaded by tenants. Supports deferred attachment:
/// files are created as pending on upload, then linked to an entity on form submission.
/// Orphaned files (IsAttached=false, older than threshold) can be cleaned up.
/// </summary>
public class TenantFile : TenantScopedEntity, IAuditable
{
    public string OriginalFileName { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string FileType { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public bool IsAttached { get; set; }

    // IAuditable
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
