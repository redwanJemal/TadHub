using SaasKit.SharedKernel.Entities;

namespace ApiManagement.Core.Entities;

/// <summary>
/// Log entry for API key usage.
/// </summary>
public class ApiKeyLog : TenantScopedEntity
{
    /// <summary>
    /// Associated API key.
    /// </summary>
    public Guid ApiKeyId { get; set; }
    public ApiKey ApiKey { get; set; } = null!;

    /// <summary>
    /// Request endpoint path.
    /// </summary>
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// HTTP method.
    /// </summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>
    /// Response status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Request duration in milliseconds.
    /// </summary>
    public int DurationMs { get; set; }

    /// <summary>
    /// Client IP address.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Request body size in bytes.
    /// </summary>
    public long? RequestSize { get; set; }

    /// <summary>
    /// Response body size in bytes.
    /// </summary>
    public long? ResponseSize { get; set; }

    /// <summary>
    /// Error message if request failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
