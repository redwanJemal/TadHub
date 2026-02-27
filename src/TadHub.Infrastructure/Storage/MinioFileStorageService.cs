using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using TadHub.Infrastructure.Settings;
using TadHub.SharedKernel.Interfaces;

namespace TadHub.Infrastructure.Storage;

/// <summary>
/// MinIO implementation of file storage service.
/// Each tenant gets an isolated bucket.
/// </summary>
public sealed class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _minioClient;
    private readonly ITenantContext _tenantContext;
    private readonly MinioSettings _settings;
    private readonly ILogger<MinioFileStorageService> _logger;

    private static readonly TimeSpan DefaultUrlExpiry = TimeSpan.FromHours(1);

    public MinioFileStorageService(
        IMinioClient minioClient,
        ITenantContext tenantContext,
        IOptions<MinioSettings> settings,
        ILogger<MinioFileStorageService> logger)
    {
        _minioClient = minioClient;
        _tenantContext = tenantContext;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> UploadAsync(
        string fileName,
        Stream stream,
        string contentType,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var bucketName = GetBucketName();
        await EnsureBucketExistsAsync(bucketName, cancellationToken);

        var objectName = GenerateObjectName(fileName);

        var putArgs = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(contentType);

        if (metadata?.Count > 0)
        {
            putArgs.WithHeaders(metadata);
        }

        await _minioClient.PutObjectAsync(putArgs, cancellationToken);

        _logger.LogDebug("Uploaded file {ObjectName} to bucket {Bucket}", objectName, bucketName);

        return objectName;
    }

    public async Task<string> GetPresignedDownloadUrlAsync(
        string fileKey,
        TimeSpan? expiresIn = null,
        CancellationToken cancellationToken = default)
    {
        var bucketName = GetBucketName();
        var expiry = (int)(expiresIn ?? DefaultUrlExpiry).TotalSeconds;

        var args = new PresignedGetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(fileKey)
            .WithExpiry(expiry);

        return await _minioClient.PresignedGetObjectAsync(args);
    }

    public async Task<string> GetPresignedUploadUrlAsync(
        string fileName,
        string contentType,
        TimeSpan? expiresIn = null,
        CancellationToken cancellationToken = default)
    {
        var bucketName = GetBucketName();
        await EnsureBucketExistsAsync(bucketName, cancellationToken);

        var objectName = GenerateObjectName(fileName);
        var expiry = (int)(expiresIn ?? DefaultUrlExpiry).TotalSeconds;

        var args = new PresignedPutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithExpiry(expiry);

        return await _minioClient.PresignedPutObjectAsync(args);
    }

    public async Task DeleteAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        var bucketName = GetBucketName();

        var args = new RemoveObjectArgs()
            .WithBucket(bucketName)
            .WithObject(fileKey);

        await _minioClient.RemoveObjectAsync(args, cancellationToken);

        _logger.LogDebug("Deleted file {ObjectName} from bucket {Bucket}", fileKey, bucketName);
    }

    public async Task<byte[]?> DownloadAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var bucketName = GetBucketName();
            using var memoryStream = new MemoryStream();

            var args = new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileKey)
                .WithCallbackStream(async (stream, ct) =>
                {
                    await stream.CopyToAsync(memoryStream, ct);
                });

            await _minioClient.GetObjectAsync(args, cancellationToken);
            return memoryStream.ToArray();
        }
        catch (Minio.Exceptions.ObjectNotFoundException)
        {
            return null;
        }
    }

    public async Task<bool> ExistsAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var bucketName = GetBucketName();

            var args = new StatObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileKey);

            await _minioClient.StatObjectAsync(args, cancellationToken);
            return true;
        }
        catch (Minio.Exceptions.ObjectNotFoundException)
        {
            return false;
        }
    }

    public async Task<FileMetadata?> GetMetadataAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var bucketName = GetBucketName();

            var args = new StatObjectArgs()
                .WithBucket(bucketName)
                .WithObject(fileKey);

            var stat = await _minioClient.StatObjectAsync(args, cancellationToken);

            return new FileMetadata(
                Key: fileKey,
                Size: stat.Size,
                ContentType: stat.ContentType,
                LastModified: stat.LastModified,
                Metadata: stat.MetaData ?? new Dictionary<string, string>());
        }
        catch (Minio.Exceptions.ObjectNotFoundException)
        {
            return null;
        }
    }

    private string GetBucketName()
    {
        if (!_tenantContext.IsResolved)
            return $"{_settings.BucketPrefix}global";

        return $"{_settings.BucketPrefix}{_tenantContext.TenantId}";
    }

    private async Task EnsureBucketExistsAsync(string bucketName, CancellationToken cancellationToken)
    {
        var existsArgs = new BucketExistsArgs().WithBucket(bucketName);
        var exists = await _minioClient.BucketExistsAsync(existsArgs, cancellationToken);

        if (!exists)
        {
            var makeArgs = new MakeBucketArgs().WithBucket(bucketName);
            await _minioClient.MakeBucketAsync(makeArgs, cancellationToken);
            _logger.LogInformation("Created bucket {Bucket}", bucketName);
        }
    }

    private static string GenerateObjectName(string fileName)
    {
        var timestamp = DateTimeOffset.UtcNow.ToString("yyyy/MM/dd");
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var sanitizedName = SanitizeFileName(fileName);
        
        return $"{timestamp}/{uniqueId}-{sanitizedName}";
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }
}
