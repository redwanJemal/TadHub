using TadHub.SharedKernel.Api;

namespace ClientManagement.Contracts.DTOs;

/// <summary>
/// Minimal reference DTO for Lead.
/// </summary>
public record LeadRefDto : IRefDto
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
    public string Source { get; init; } = string.Empty;
}

/// <summary>
/// Full Lead DTO.
/// </summary>
public record LeadDto : IRefDto
{
    public Guid Id { get; init; }
    
    /// <summary>
    /// Client (after conversion). Null if not yet converted.
    /// Uses RefDto when include=client is not specified.
    /// </summary>
    public ClientRefDto? Client { get; init; }
    
    /// <summary>
    /// Lead source: WalkIn, Phone, Online, Referral, SocialMedia.
    /// </summary>
    public string Source { get; init; } = string.Empty;
    
    /// <summary>
    /// Lead status: New, Contacted, Qualified, Converted, Lost.
    /// </summary>
    public string Status { get; init; } = string.Empty;
    
    /// <summary>
    /// Notes about the lead.
    /// </summary>
    public string? Notes { get; init; }
    
    /// <summary>
    /// Assigned sales agent (RefDto pattern).
    /// Null means not assigned.
    /// </summary>
    public UserRefDto? AssignedTo { get; init; }
    
    /// <summary>
    /// Contact name (before conversion to client).
    /// </summary>
    public string? ContactName { get; init; }
    
    /// <summary>
    /// Contact phone.
    /// </summary>
    public string? ContactPhone { get; init; }
    
    /// <summary>
    /// Contact email.
    /// </summary>
    public string? ContactEmail { get; init; }
    
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>
/// Minimal user reference for nested objects.
/// </summary>
public record UserRefDto : IRefDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Email { get; init; }
}

/// <summary>
/// Request to create a new lead.
/// </summary>
public record CreateLeadRequest
{
    /// <summary>
    /// Lead source: WalkIn, Phone, Online, Referral, SocialMedia.
    /// </summary>
    public string Source { get; init; } = string.Empty;
    
    /// <summary>
    /// Contact name.
    /// </summary>
    public string? ContactName { get; init; }
    
    /// <summary>
    /// Contact phone.
    /// </summary>
    public string? ContactPhone { get; init; }
    
    /// <summary>
    /// Contact email.
    /// </summary>
    public string? ContactEmail { get; init; }
    
    /// <summary>
    /// Notes about the lead.
    /// </summary>
    public string? Notes { get; init; }
    
    /// <summary>
    /// Assign to user ID (optional).
    /// </summary>
    public Guid? AssignedToUserId { get; init; }
}

/// <summary>
/// Request to update a lead.
/// </summary>
public record UpdateLeadRequest
{
    public string? Status { get; init; }
    public string? Notes { get; init; }
    public Guid? AssignedToUserId { get; init; }
    public string? ContactName { get; init; }
    public string? ContactPhone { get; init; }
    public string? ContactEmail { get; init; }
}

/// <summary>
/// Request to convert a lead to a client.
/// </summary>
public record ConvertLeadRequest
{
    /// <summary>
    /// The client registration details.
    /// </summary>
    public CreateClientRequest Client { get; init; } = new();
}

/// <summary>
/// Communication log DTO.
/// </summary>
public record CommunicationLogDto
{
    public Guid Id { get; init; }
    public string Channel { get; init; } = string.Empty;
    public string Direction { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public UserRefDto? LoggedBy { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
}
