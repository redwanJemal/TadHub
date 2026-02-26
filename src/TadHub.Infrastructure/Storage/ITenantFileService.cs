namespace TadHub.Infrastructure.Storage;

/// <summary>
/// Service for managing tenant file uploads with tracking.
/// Files are created as pending on upload, then attached to entities on form submission.
/// </summary>
public interface ITenantFileService
{
    Task<TenantFileDto> UploadAsync(Guid tenantId, string originalFileName, Stream stream, string contentType, long fileSize, string fileType, CancellationToken ct = default);
    Task<TenantFileDto?> GetByIdAsync(Guid fileId, CancellationToken ct = default);
    Task AttachToEntityAsync(Guid fileId, string entityType, Guid entityId, CancellationToken ct = default);
    Task AttachMultipleAsync(IEnumerable<Guid> fileIds, string entityType, Guid entityId, CancellationToken ct = default);
    Task<string?> GetStorageKeyAsync(Guid fileId, CancellationToken ct = default);
    Task<string?> GetPresignedUrlAsync(Guid fileId, CancellationToken ct = default);
    Task<string?> GetPresignedUrlByKeyAsync(string storageKey, CancellationToken ct = default);
}

public sealed record TenantFileDto(
    Guid Id,
    string OriginalFileName,
    string StorageKey,
    string Url,
    string FileType,
    long FileSizeBytes);
