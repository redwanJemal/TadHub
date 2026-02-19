namespace Portal.Contracts.DTOs;

/// <summary>
/// DTO for portal data.
/// </summary>
public record PortalDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Subdomain { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }

    // Branding
    public string? PrimaryColor { get; init; }
    public string? SecondaryColor { get; init; }
    public string? LogoUrl { get; init; }
    public string? FaviconUrl { get; init; }

    // SEO
    public string? SeoTitle { get; init; }
    public string? SeoDescription { get; init; }

    // Auth
    public bool AllowPublicRegistration { get; init; }
    public bool RequireEmailVerification { get; init; }
    public bool EnableSso { get; init; }

    // Stats
    public int UserCount { get; init; }
    public int PageCount { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    /// <summary>
    /// Full portal URL.
    /// </summary>
    public string Url => $"https://{Subdomain}.portal.example.com";
}

/// <summary>
/// Request to create a portal.
/// </summary>
public record CreatePortalRequest
{
    public string Name { get; init; } = string.Empty;
    public string Subdomain { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? PrimaryColor { get; init; }
    public string? SecondaryColor { get; init; }
    public bool AllowPublicRegistration { get; init; } = true;
}

/// <summary>
/// Request to update a portal.
/// </summary>
public record UpdatePortalRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool? IsActive { get; init; }
    public string? PrimaryColor { get; init; }
    public string? SecondaryColor { get; init; }
    public string? LogoUrl { get; init; }
    public string? FaviconUrl { get; init; }
    public string? SeoTitle { get; init; }
    public string? SeoDescription { get; init; }
    public bool? AllowPublicRegistration { get; init; }
    public bool? RequireEmailVerification { get; init; }
}

/// <summary>
/// DTO for portal domain.
/// </summary>
public record PortalDomainDto
{
    public Guid Id { get; init; }
    public Guid PortalId { get; init; }
    public string Domain { get; init; } = string.Empty;
    public bool IsPrimary { get; init; }
    public bool IsVerified { get; init; }
    public string? VerificationToken { get; init; }
    public DateTimeOffset? VerifiedAt { get; init; }
    public string SslStatus { get; init; } = "pending";
}
