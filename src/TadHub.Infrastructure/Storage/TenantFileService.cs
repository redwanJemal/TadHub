using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Persistence;

namespace TadHub.Infrastructure.Storage;

public sealed class TenantFileService : ITenantFileService
{
    private readonly AppDbContext _db;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<TenantFileService> _logger;

    private static readonly Dictionary<string, (string[] AllowedTypes, long MaxSize)> FileTypeRules = new()
    {
        ["photo"] = (["image/jpeg", "image/png", "image/webp"], 5 * 1024 * 1024),
        ["video"] = (["video/mp4", "video/webm"], 50 * 1024 * 1024),
        ["passport"] = (["application/pdf", "image/jpeg", "image/png"], 10 * 1024 * 1024),
    };

    public TenantFileService(
        AppDbContext db,
        IFileStorageService fileStorageService,
        ILogger<TenantFileService> logger)
    {
        _db = db;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    public async Task<TenantFileDto> UploadAsync(
        Guid tenantId,
        string originalFileName,
        Stream stream,
        string contentType,
        long fileSize,
        string fileType,
        CancellationToken ct = default)
    {
        // Upload to MinIO
        var storageKey = await _fileStorageService.UploadAsync(
            originalFileName, stream, contentType, cancellationToken: ct);

        // Create tracking record
        var tenantFile = new TenantFile
        {
            TenantId = tenantId,
            OriginalFileName = originalFileName,
            StorageKey = storageKey,
            ContentType = contentType,
            FileSizeBytes = fileSize,
            FileType = fileType,
            IsAttached = false,
        };

        _db.Set<TenantFile>().Add(tenantFile);
        await _db.SaveChangesAsync(ct);

        // Generate presigned URL for immediate use
        var presignedUrl = await _fileStorageService.GetPresignedDownloadUrlAsync(
            storageKey, TimeSpan.FromHours(1), ct);

        _logger.LogDebug("Uploaded tenant file {FileId} ({FileType}) to {StorageKey}",
            tenantFile.Id, fileType, storageKey);

        return new TenantFileDto(
            tenantFile.Id,
            tenantFile.OriginalFileName,
            tenantFile.StorageKey,
            presignedUrl,
            tenantFile.FileType,
            tenantFile.FileSizeBytes);
    }

    public async Task<TenantFileDto?> GetByIdAsync(Guid fileId, CancellationToken ct = default)
    {
        var file = await _db.Set<TenantFile>().FindAsync([fileId], ct);
        if (file is null) return null;

        string url;
        try
        {
            url = await _fileStorageService.GetPresignedDownloadUrlAsync(
                file.StorageKey, TimeSpan.FromHours(1), ct);
        }
        catch
        {
            url = file.StorageKey;
        }

        return new TenantFileDto(file.Id, file.OriginalFileName, file.StorageKey, url, file.FileType, file.FileSizeBytes);
    }

    public async Task AttachToEntityAsync(Guid fileId, string entityType, Guid entityId, CancellationToken ct = default)
    {
        var file = await _db.Set<TenantFile>().FindAsync([fileId], ct);
        if (file is null) return;

        file.EntityType = entityType;
        file.EntityId = entityId;
        file.IsAttached = true;
        await _db.SaveChangesAsync(ct);

        _logger.LogDebug("Attached file {FileId} to {EntityType}/{EntityId}", fileId, entityType, entityId);
    }

    public async Task AttachMultipleAsync(IEnumerable<Guid> fileIds, string entityType, Guid entityId, CancellationToken ct = default)
    {
        var ids = fileIds.ToList();
        if (ids.Count == 0) return;

        var files = await _db.Set<TenantFile>()
            .Where(f => ids.Contains(f.Id))
            .ToListAsync(ct);

        foreach (var file in files)
        {
            file.EntityType = entityType;
            file.EntityId = entityId;
            file.IsAttached = true;
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<string?> GetStorageKeyAsync(Guid fileId, CancellationToken ct = default)
    {
        var file = await _db.Set<TenantFile>().FindAsync([fileId], ct);
        return file?.StorageKey;
    }

    public async Task<string?> GetPresignedUrlAsync(Guid fileId, CancellationToken ct = default)
    {
        var file = await _db.Set<TenantFile>().FindAsync([fileId], ct);
        if (file is null) return null;

        try
        {
            return await _fileStorageService.GetPresignedDownloadUrlAsync(
                file.StorageKey, TimeSpan.FromHours(1), ct);
        }
        catch
        {
            return file.StorageKey;
        }
    }

    public async Task<string?> GetPresignedUrlByKeyAsync(string storageKey, CancellationToken ct = default)
    {
        try
        {
            return await _fileStorageService.GetPresignedDownloadUrlAsync(
                storageKey, TimeSpan.FromHours(1), ct);
        }
        catch
        {
            return storageKey;
        }
    }

    /// <summary>
    /// Gets the validation rules for a file type.
    /// </summary>
    public static (string[] AllowedTypes, long MaxSize)? GetValidationRules(string fileType)
    {
        return FileTypeRules.TryGetValue(fileType, out var rules) ? rules : null;
    }
}
