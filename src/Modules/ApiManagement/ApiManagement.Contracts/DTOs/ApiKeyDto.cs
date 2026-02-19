namespace ApiManagement.Contracts.DTOs;

public record ApiKeyDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Prefix { get; init; } = string.Empty;
    public List<string>? Permissions { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset? LastUsedAt { get; init; }
    public long RequestCount { get; init; }
    public int? RateLimitPerMinute { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public record ApiKeyWithSecretDto : ApiKeyDto
{
    /// <summary>
    /// The full API key (only returned on creation).
    /// </summary>
    public string Secret { get; init; } = string.Empty;
}

public record CreateApiKeyRequest
{
    public string Name { get; init; } = string.Empty;
    public List<string>? Permissions { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
    public int? RateLimitPerMinute { get; init; }
}

public record ApiKeyLogDto
{
    public Guid Id { get; init; }
    public Guid ApiKeyId { get; init; }
    public string Endpoint { get; init; } = string.Empty;
    public string Method { get; init; } = string.Empty;
    public int StatusCode { get; init; }
    public int DurationMs { get; init; }
    public string? IpAddress { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
