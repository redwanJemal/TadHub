using TadHub.SharedKernel.Entities;

namespace Portal.Core.Entities;

/// <summary>
/// API key for portal-level API access.
/// </summary>
public class PortalApiKey : TenantScopedEntity
{
    /// <summary>
    /// The portal this API key belongs to.
    /// </summary>
    public Guid PortalId { get; set; }
    public Portal Portal { get; set; } = null!;

    /// <summary>
    /// Key name/description.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The API key prefix (first 8 chars, for display).
    /// </summary>
    public string KeyPrefix { get; set; } = string.Empty;

    /// <summary>
    /// Hashed API key.
    /// </summary>
    public string KeyHash { get; set; } = string.Empty;

    /// <summary>
    /// Whether the key is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When the key expires (null = never).
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    /// Last time the key was used.
    /// </summary>
    public DateTimeOffset? LastUsedAt { get; set; }

    /// <summary>
    /// Number of times the key has been used.
    /// </summary>
    public long UsageCount { get; set; } = 0;

    /// <summary>
    /// Allowed scopes/permissions (JSON array).
    /// </summary>
    public string? Scopes { get; set; }

    /// <summary>
    /// IP whitelist (JSON array, null = all IPs allowed).
    /// </summary>
    public string? AllowedIps { get; set; }

    /// <summary>
    /// Who created this key.
    /// </summary>
    public Guid CreatedByUserId { get; set; }
}
