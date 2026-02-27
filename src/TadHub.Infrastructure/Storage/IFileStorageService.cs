namespace TadHub.Infrastructure.Storage;

/// <summary>
/// File storage service interface for tenant-scoped file operations.
/// Each tenant has an isolated bucket/container.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file to storage.
    /// </summary>
    /// <param name="fileName">The file name (can include path prefix).</param>
    /// <param name="stream">The file content stream.</param>
    /// <param name="contentType">The MIME type of the file.</param>
    /// <param name="metadata">Optional metadata to store with the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The storage key/path of the uploaded file.</returns>
    Task<string> UploadAsync(
        string fileName,
        Stream stream,
        string contentType,
        Dictionary<string, string>? metadata = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a presigned URL for downloading a file.
    /// </summary>
    /// <param name="fileKey">The storage key/path of the file.</param>
    /// <param name="expiresIn">How long the URL should be valid.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A presigned URL for downloading.</returns>
    Task<string> GetPresignedDownloadUrlAsync(
        string fileKey,
        TimeSpan? expiresIn = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a presigned URL for uploading a file.
    /// </summary>
    /// <param name="fileName">The file name to upload.</param>
    /// <param name="contentType">The MIME type of the file.</param>
    /// <param name="expiresIn">How long the URL should be valid.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A presigned URL for uploading.</returns>
    Task<string> GetPresignedUploadUrlAsync(
        string fileName,
        string contentType,
        TimeSpan? expiresIn = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file from storage.
    /// </summary>
    /// <param name="fileKey">The storage key/path of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(string fileKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a file exists.
    /// </summary>
    /// <param name="fileKey">The storage key/path of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the file exists.</returns>
    Task<bool> ExistsAsync(string fileKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads file content as a byte array.
    /// Returns null if the file does not exist.
    /// </summary>
    /// <param name="fileKey">The storage key/path of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The file bytes, or null if not found.</returns>
    Task<byte[]?> DownloadAsync(string fileKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets file metadata.
    /// </summary>
    /// <param name="fileKey">The storage key/path of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>File metadata including size, content type, and custom metadata.</returns>
    Task<FileMetadata?> GetMetadataAsync(string fileKey, CancellationToken cancellationToken = default);
}

/// <summary>
/// File metadata returned by storage operations.
/// </summary>
public sealed record FileMetadata(
    string Key,
    long Size,
    string ContentType,
    DateTimeOffset LastModified,
    Dictionary<string, string> Metadata);
