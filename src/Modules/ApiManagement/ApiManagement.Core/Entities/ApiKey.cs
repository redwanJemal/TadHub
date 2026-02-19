using SaasKit.SharedKernel.Entities;

namespace ApiManagement.Core.Entities;

/// <summary>
/// Represents an API key for programmatic access.
/// </summary>
public class ApiKey : TenantScopedEntity
{
    /// <summary>
    /// Human-readable name for the key.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Key prefix for display (first 8 chars).
    /// </summary>
    public string Prefix { get; set; } = string.Empty;

    /// <summary>
    /// SHA256 hash of the full key.
    /// </summary>
    public string KeyHash { get; set; } = string.Empty;

    /// <summary>
    /// Allowed permissions/scopes (JSON array).
    /// </summary>
    public string? Permissions { get; set; }

    /// <summary>
    /// When the key expires (null = never).
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>
    /// Whether the key is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Last time the key was used.
    /// </summary>
    public DateTimeOffset? LastUsedAt { get; set; }

    /// <summary>
    /// Total number of requests made with this key.
    /// </summary>
    public long RequestCount { get; set; } = 0;

    /// <summary>
    /// Rate limit (requests per minute). Null = default.
    /// </summary>
    public int? RateLimitPerMinute { get; set; }

    /// <summary>
    /// User who created this key.
    /// </summary>
    public Guid CreatedByUserId { get; set; }

    /// <summary>
    /// API key logs.
    /// </summary>
    public ICollection<ApiKeyLog> Logs { get; set; } = new List<ApiKeyLog>();
}
