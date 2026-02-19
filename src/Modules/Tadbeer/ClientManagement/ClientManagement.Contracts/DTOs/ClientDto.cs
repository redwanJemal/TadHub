using TadHub.SharedKernel.Api;

namespace ClientManagement.Contracts.DTOs;

/// <summary>
/// Minimal reference DTO for Client.
/// Used in nested objects when include=client is NOT specified.
/// </summary>
public record ClientRefDto : IRefDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string EmiratesId { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
}

/// <summary>
/// Full Client DTO with all details.
/// Used when include=client IS specified or on detail endpoints.
/// </summary>
public record ClientDto : IRefDto
{
    public Guid Id { get; init; }
    
    /// <summary>
    /// UAE Emirates ID number.
    /// </summary>
    public string EmiratesId { get; init; } = string.Empty;
    
    /// <summary>
    /// Full name in English.
    /// </summary>
    public string FullNameEn { get; init; } = string.Empty;
    
    /// <summary>
    /// Full name in Arabic.
    /// </summary>
    public string FullNameAr { get; init; } = string.Empty;
    
    /// <summary>
    /// Passport number.
    /// </summary>
    public string? PassportNumber { get; init; }
    
    /// <summary>
    /// Nationality (e.g., "UAE", "India", "Philippines").
    /// </summary>
    public string Nationality { get; init; } = string.Empty;
    
    /// <summary>
    /// Client category: Local, Expat, Investor, VIP.
    /// </summary>
    public string Category { get; init; } = string.Empty;
    
    /// <summary>
    /// Contact phone number.
    /// </summary>
    public string? Phone { get; init; }
    
    /// <summary>
    /// Contact email.
    /// </summary>
    public string? Email { get; init; }
    
    /// <summary>
    /// Sponsor file status: Open, Pending, Active, Blocked.
    /// </summary>
    public string SponsorFileStatus { get; init; } = string.Empty;
    
    /// <summary>
    /// UAE Emirate of residence.
    /// </summary>
    public string? Emirate { get; init; }
    
    /// <summary>
    /// Whether client documents have been verified.
    /// </summary>
    public bool IsVerified { get; init; }
    
    /// <summary>
    /// When the client was verified.
    /// </summary>
    public DateTimeOffset? VerifiedAt { get; init; }
    
    /// <summary>
    /// Reason if client is blocked.
    /// </summary>
    public string? BlockedReason { get; init; }
    
    /// <summary>
    /// Additional notes.
    /// </summary>
    public string? Notes { get; init; }
    
    /// <summary>
    /// When the client was registered.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
    
    /// <summary>
    /// Documents (only included when requested).
    /// Null = not included, [] = included but empty.
    /// </summary>
    public List<ClientDocumentDto>? Documents { get; init; }
    
    /// <summary>
    /// Discount cards (only included when requested).
    /// </summary>
    public List<DiscountCardDto>? DiscountCards { get; init; }
}

/// <summary>
/// Client document DTO.
/// </summary>
public record ClientDocumentDto
{
    public Guid Id { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string FileUrl { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public DateTimeOffset? ExpiresAt { get; init; }
    public bool IsVerified { get; init; }
    public DateTimeOffset UploadedAt { get; init; }
}

/// <summary>
/// Discount card DTO.
/// </summary>
public record DiscountCardDto
{
    public Guid Id { get; init; }
    public string CardType { get; init; } = string.Empty;
    public string CardNumber { get; init; } = string.Empty;
    public decimal DiscountPercentage { get; init; }
    public DateTimeOffset? ValidUntil { get; init; }
}
