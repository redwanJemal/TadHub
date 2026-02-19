using TadHub.SharedKernel.Entities;

namespace Worker.Core.Entities;

/// <summary>
/// Worker media (photos, videos, documents).
/// </summary>
public class WorkerMedia : TenantScopedEntity
{
    /// <summary>
    /// Worker ID FK.
    /// </summary>
    public Guid WorkerId { get; set; }

    /// <summary>
    /// Worker navigation.
    /// </summary>
    public Worker? Worker { get; set; }

    /// <summary>
    /// Media type.
    /// </summary>
    public MediaType MediaType { get; set; }

    /// <summary>
    /// File URL.
    /// </summary>
    public string FileUrl { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is the primary photo/video.
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// When uploaded.
    /// </summary>
    public DateTimeOffset UploadedAt { get; set; }

    /// <summary>
    /// User who uploaded.
    /// </summary>
    public Guid? UploadedByUserId { get; set; }
}
