namespace ClientManagement.Contracts.DTOs;

/// <summary>
/// Request to register a new client.
/// </summary>
public record CreateClientRequest
{
    /// <summary>
    /// UAE Emirates ID number (required).
    /// </summary>
    public string EmiratesId { get; init; } = string.Empty;
    
    /// <summary>
    /// Full name in English (required).
    /// </summary>
    public string FullNameEn { get; init; } = string.Empty;
    
    /// <summary>
    /// Full name in Arabic (required).
    /// </summary>
    public string FullNameAr { get; init; } = string.Empty;
    
    /// <summary>
    /// Passport number (optional).
    /// </summary>
    public string? PassportNumber { get; init; }
    
    /// <summary>
    /// Nationality (required).
    /// </summary>
    public string Nationality { get; init; } = string.Empty;
    
    /// <summary>
    /// Contact phone number.
    /// </summary>
    public string? Phone { get; init; }
    
    /// <summary>
    /// Contact email.
    /// </summary>
    public string? Email { get; init; }
    
    /// <summary>
    /// UAE Emirate of residence.
    /// </summary>
    public string? Emirate { get; init; }
    
    /// <summary>
    /// Optional category override. If not provided, auto-detected from EmiratesId.
    /// Requires clients.manage permission to override.
    /// </summary>
    public string? CategoryOverride { get; init; }
    
    /// <summary>
    /// URL to salary certificate document.
    /// </summary>
    public string? SalaryCertificateUrl { get; init; }
    
    /// <summary>
    /// URL to Ejari/tenancy contract document.
    /// </summary>
    public string? EjariUrl { get; init; }
    
    /// <summary>
    /// Additional notes.
    /// </summary>
    public string? Notes { get; init; }
}

/// <summary>
/// Request to update an existing client.
/// </summary>
public record UpdateClientRequest
{
    public string? FullNameEn { get; init; }
    public string? FullNameAr { get; init; }
    public string? PassportNumber { get; init; }
    public string? Nationality { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? Emirate { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Request to block a client.
/// </summary>
public record BlockClientRequest
{
    /// <summary>
    /// Reason for blocking (required).
    /// </summary>
    public string Reason { get; init; } = string.Empty;
}

/// <summary>
/// Request to add a document to a client.
/// </summary>
public record AddDocumentRequest
{
    /// <summary>
    /// Document type: EmiratesId, Passport, SalaryCertificate, EjariContract, TenancyContract, Other.
    /// </summary>
    public string DocumentType { get; init; } = string.Empty;
    
    /// <summary>
    /// URL to the uploaded document file.
    /// </summary>
    public string FileUrl { get; init; } = string.Empty;
    
    /// <summary>
    /// Original file name.
    /// </summary>
    public string FileName { get; init; } = string.Empty;
    
    /// <summary>
    /// Document expiry date (if applicable).
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; init; }
}

/// <summary>
/// Request to log a communication with a client.
/// </summary>
public record AddCommunicationRequest
{
    /// <summary>
    /// Channel: Phone, WhatsApp, Email, WalkIn.
    /// </summary>
    public string Channel { get; init; } = string.Empty;
    
    /// <summary>
    /// Direction: Inbound, Outbound.
    /// </summary>
    public string Direction { get; init; } = string.Empty;
    
    /// <summary>
    /// Summary of the communication.
    /// </summary>
    public string Summary { get; init; } = string.Empty;
}
