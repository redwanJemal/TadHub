using ClientManagement.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace ClientManagement.Contracts;

/// <summary>
/// Service interface for client (employer) management.
/// </summary>
public interface IClientService
{
    #region Client Operations

    /// <summary>
    /// Registers a new client.
    /// Auto-detects category from EmiratesId unless CategoryOverride is provided.
    /// Publishes ClientRegisteredEvent on success.
    /// </summary>
    Task<Result<ClientDto>> RegisterAsync(CreateClientRequest request, CancellationToken ct = default);

    /// <summary>
    /// Gets a client by ID.
    /// </summary>
    /// <param name="id">Client ID</param>
    /// <param name="includes">Relations to include (documents, discountCards)</param>
    Task<Result<ClientDto>> GetByIdAsync(Guid id, IncludeSet includes, CancellationToken ct = default);

    /// <summary>
    /// Updates a client.
    /// </summary>
    Task<Result<ClientDto>> UpdateAsync(Guid id, UpdateClientRequest request, CancellationToken ct = default);

    /// <summary>
    /// Verifies a client's documents.
    /// Sets IsVerified=true and publishes ClientVerifiedEvent.
    /// This unblocks contract creation for the client.
    /// </summary>
    Task<Result<ClientDto>> VerifyAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Blocks a client.
    /// Sets SponsorFileStatus=Blocked and publishes ClientBlockedEvent.
    /// All pending contracts for this client are paused.
    /// </summary>
    Task<Result<ClientDto>> BlockAsync(Guid id, string reason, CancellationToken ct = default);

    /// <summary>
    /// Unblocks a previously blocked client.
    /// </summary>
    Task<Result<ClientDto>> UnblockAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Lists clients with filtering, sorting, and pagination.
    /// 
    /// Supported filters:
    /// - filter[category]=local&amp;filter[category]=expat (array)
    /// - filter[sponsorFileStatus]=active
    /// - filter[isVerified]=true
    /// - filter[nationality]=UAE
    /// - filter[emirate]=dubai
    /// - filter[createdAt][gte]=2026-01-01
    /// </summary>
    Task<PagedList<ClientDto>> ListAsync(QueryParameters query, CancellationToken ct = default);

    /// <summary>
    /// Searches clients by name or Emirates ID.
    /// </summary>
    Task<PagedList<ClientRefDto>> SearchAsync(string searchTerm, QueryParameters query, CancellationToken ct = default);

    #endregion

    #region Document Operations

    /// <summary>
    /// Gets all documents for a client.
    /// </summary>
    Task<Result<List<ClientDocumentDto>>> GetDocumentsAsync(Guid clientId, CancellationToken ct = default);

    /// <summary>
    /// Adds a document to a client.
    /// </summary>
    Task<Result<ClientDocumentDto>> AddDocumentAsync(Guid clientId, AddDocumentRequest request, CancellationToken ct = default);

    /// <summary>
    /// Verifies a specific document.
    /// </summary>
    Task<Result<ClientDocumentDto>> VerifyDocumentAsync(Guid clientId, Guid documentId, CancellationToken ct = default);

    /// <summary>
    /// Deletes a document.
    /// </summary>
    Task<Result> DeleteDocumentAsync(Guid clientId, Guid documentId, CancellationToken ct = default);

    #endregion

    #region Communication Log Operations

    /// <summary>
    /// Gets communication logs for a client.
    /// </summary>
    Task<PagedList<CommunicationLogDto>> GetCommunicationsAsync(Guid clientId, QueryParameters query, CancellationToken ct = default);

    /// <summary>
    /// Adds a communication log entry.
    /// </summary>
    Task<Result<CommunicationLogDto>> AddCommunicationAsync(Guid clientId, AddCommunicationRequest request, CancellationToken ct = default);

    #endregion

    #region Discount Card Operations

    /// <summary>
    /// Gets discount cards for a client.
    /// </summary>
    Task<Result<List<DiscountCardDto>>> GetDiscountCardsAsync(Guid clientId, CancellationToken ct = default);

    /// <summary>
    /// Adds a discount card to a client.
    /// </summary>
    Task<Result<DiscountCardDto>> AddDiscountCardAsync(Guid clientId, AddDiscountCardRequest request, CancellationToken ct = default);

    #endregion
}

/// <summary>
/// Service interface for lead management.
/// </summary>
public interface ILeadService
{
    /// <summary>
    /// Creates a new lead.
    /// </summary>
    Task<Result<LeadDto>> CreateAsync(CreateLeadRequest request, CancellationToken ct = default);

    /// <summary>
    /// Gets a lead by ID.
    /// </summary>
    Task<Result<LeadDto>> GetByIdAsync(Guid id, IncludeSet includes, CancellationToken ct = default);

    /// <summary>
    /// Updates a lead.
    /// </summary>
    Task<Result<LeadDto>> UpdateAsync(Guid id, UpdateLeadRequest request, CancellationToken ct = default);

    /// <summary>
    /// Converts a lead to a client.
    /// Creates the client, links the lead, and sets lead status to Converted.
    /// </summary>
    Task<Result<ClientDto>> ConvertToClientAsync(Guid leadId, ConvertLeadRequest request, CancellationToken ct = default);

    /// <summary>
    /// Lists leads with filtering, sorting, and pagination.
    /// 
    /// Supported filters:
    /// - filter[status]=new&amp;filter[status]=contacted (array)
    /// - filter[source]=walkIn
    /// - filter[assignedToUserId]=...
    /// - filter[createdAt][gte]=2026-01-01
    /// </summary>
    Task<PagedList<LeadDto>> ListAsync(QueryParameters query, CancellationToken ct = default);

    /// <summary>
    /// Gets lead conversion funnel statistics.
    /// </summary>
    Task<LeadFunnelStats> GetFunnelStatsAsync(DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default);
}

/// <summary>
/// Lead conversion funnel statistics.
/// </summary>
public record LeadFunnelStats
{
    public int TotalLeads { get; init; }
    public int NewLeads { get; init; }
    public int ContactedLeads { get; init; }
    public int QualifiedLeads { get; init; }
    public int ConvertedLeads { get; init; }
    public int LostLeads { get; init; }
    public decimal ConversionRate { get; init; }
}

/// <summary>
/// Request to add a discount card.
/// </summary>
public record AddDiscountCardRequest
{
    /// <summary>
    /// Card type: Saada, Fazaa, Custom.
    /// </summary>
    public string CardType { get; init; } = string.Empty;
    
    /// <summary>
    /// Card number.
    /// </summary>
    public string CardNumber { get; init; } = string.Empty;
    
    /// <summary>
    /// Discount percentage (0-100).
    /// </summary>
    public decimal DiscountPercentage { get; init; }
    
    /// <summary>
    /// Card validity date.
    /// </summary>
    public DateTimeOffset? ValidUntil { get; init; }
}
