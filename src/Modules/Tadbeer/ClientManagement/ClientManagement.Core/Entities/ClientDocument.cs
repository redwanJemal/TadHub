using TadHub.SharedKernel.Entities;

namespace ClientManagement.Core.Entities;

/// <summary>
/// Document uploaded for a client.
/// </summary>
public class ClientDocument : TenantScopedEntity
{
    /// <summary>
    /// Client this document belongs to.
    /// </summary>
    public Guid ClientId { get; set; }

    /// <summary>
    /// Navigation property for client.
    /// </summary>
    public Client? Client { get; set; }

    /// <summary>
    /// Type of document.
    /// </summary>
    public ClientDocumentType DocumentType { get; set; }

    /// <summary>
    /// URL to the stored file (MinIO/S3).
    /// </summary>
    public string FileUrl { get; set; } = string.Empty;

    /// <summary>
    /// Original file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Document expiry date (for IDs, passports, etc.).
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    /// Whether the document has been verified.
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// When the document was uploaded.
    /// </summary>
    public DateTimeOffset UploadedAt { get; set; }

    /// <summary>
    /// User who uploaded the document.
    /// </summary>
    public Guid? UploadedByUserId { get; set; }

    /// <summary>
    /// When the document was verified.
    /// </summary>
    public DateTimeOffset? VerifiedAt { get; set; }

    /// <summary>
    /// User who verified the document.
    /// </summary>
    public Guid? VerifiedByUserId { get; set; }
}
