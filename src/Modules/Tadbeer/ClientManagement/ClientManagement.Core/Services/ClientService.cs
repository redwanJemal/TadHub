using ClientManagement.Contracts;
using ClientManagement.Contracts.DTOs;
using ClientManagement.Core.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Enums;
using TadHub.SharedKernel.Events.Tadbeer.Client;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;

namespace ClientManagement.Core.Services;

/// <summary>
/// Service implementation for client management.
/// </summary>
public class ClientService : IClientService
{
    private readonly AppDbContext _db;
    private readonly IPublishEndpoint _publisher;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly ILogger<ClientService> _logger;

    // Filterable fields for ListAsync
    private static readonly Dictionary<string, Func<Client, object?>> FilterableFields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["category"] = x => x.Category.ToString().ToLowerInvariant(),
        ["sponsorFileStatus"] = x => x.SponsorFileStatus.ToString().ToLowerInvariant(),
        ["isVerified"] = x => x.IsVerified,
        ["nationality"] = x => x.Nationality,
        ["emirate"] = x => x.Emirate?.ToString().ToLowerInvariant(),
        ["createdAt"] = x => x.CreatedAt
    };

    // Sortable fields
    private static readonly Dictionary<string, Func<Client, object?>> SortableFields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["createdAt"] = x => x.CreatedAt,
        ["fullNameEn"] = x => x.FullNameEn,
        ["fullNameAr"] = x => x.FullNameAr,
        ["category"] = x => x.Category,
        ["nationality"] = x => x.Nationality
    };

    public ClientService(
        AppDbContext db,
        IPublishEndpoint publisher,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        IClock clock,
        ILogger<ClientService> logger)
    {
        _db = db;
        _publisher = publisher;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _clock = clock;
        _logger = logger;
    }

    #region Client Operations

    public async Task<Result<ClientDto>> RegisterAsync(CreateClientRequest request, CancellationToken ct = default)
    {
        // Validate Emirates ID format
        if (!CategoryDetector.IsValidEmiratesId(request.EmiratesId))
        {
            return Result<ClientDto>.Failure("Invalid Emirates ID format", "INVALID_EMIRATES_ID");
        }

        // Check for duplicate Emirates ID within tenant
        var exists = await _db.Set<Client>()
            .AnyAsync(c => c.EmiratesId == request.EmiratesId, ct);

        if (exists)
        {
            return Result<ClientDto>.Failure("Client with this Emirates ID already exists", "DUPLICATE_EMIRATES_ID");
        }

        // Detect category (or use override if provided)
        var category = !string.IsNullOrEmpty(request.CategoryOverride)
            ? Enum.Parse<ClientCategory>(request.CategoryOverride, ignoreCase: true)
            : CategoryDetector.DetectFromEmiratesId(request.EmiratesId);

        // Parse emirate if provided
        Emirate? emirate = null;
        if (!string.IsNullOrEmpty(request.Emirate))
        {
            emirate = Enum.Parse<Emirate>(request.Emirate, ignoreCase: true);
        }

        var client = new Client
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId,
            EmiratesId = CategoryDetector.FormatEmiratesId(request.EmiratesId),
            FullNameEn = request.FullNameEn,
            FullNameAr = request.FullNameAr,
            PassportNumber = request.PassportNumber,
            Nationality = request.Nationality,
            Phone = request.Phone,
            Email = request.Email,
            Category = category,
            Emirate = emirate,
            SponsorFileStatus = SponsorFileStatus.Open,
            Notes = request.Notes,
            CreatedBy = _currentUser.UserId
        };

        _db.Set<Client>().Add(client);
        await _db.SaveChangesAsync(ct);

        // Publish event
        await _publisher.Publish(new ClientRegisteredEvent
        {
            TenantId = _tenantContext.TenantId,
            ClientId = client.Id,
            EmiratesId = client.EmiratesId,
            FullNameEn = client.FullNameEn,
            FullNameAr = client.FullNameAr,
            Category = client.Category.ToString(),
            RegisteredByUserId = _currentUser.UserId
        }, ct);

        _logger.LogInformation("Client {ClientId} registered with Emirates ID {EmiratesId}", 
            client.Id, client.EmiratesId);

        return Result<ClientDto>.Success(MapToDto(client, IncludeResolver.Parse(null)));
    }

    public async Task<Result<ClientDto>> GetByIdAsync(Guid id, IncludeSet includes, CancellationToken ct = default)
    {
        var query = _db.Set<Client>().AsQueryable();

        // Include relations if requested
        if (includes.Has("documents"))
            query = query.Include(c => c.Documents);
        if (includes.Has("discountCards"))
            query = query.Include(c => c.DiscountCards);

        var client = await query.FirstOrDefaultAsync(c => c.Id == id, ct);

        if (client == null)
            return Result<ClientDto>.Failure("Client not found", "NOT_FOUND");

        return Result<ClientDto>.Success(MapToDto(client, includes));
    }

    public async Task<Result<ClientDto>> UpdateAsync(Guid id, UpdateClientRequest request, CancellationToken ct = default)
    {
        var client = await _db.Set<Client>().FindAsync([id], ct);

        if (client == null)
            return Result<ClientDto>.Failure("Client not found", "NOT_FOUND");

        // Update fields if provided
        if (request.FullNameEn != null) client.FullNameEn = request.FullNameEn;
        if (request.FullNameAr != null) client.FullNameAr = request.FullNameAr;
        if (request.PassportNumber != null) client.PassportNumber = request.PassportNumber;
        if (request.Nationality != null) client.Nationality = request.Nationality;
        if (request.Phone != null) client.Phone = request.Phone;
        if (request.Email != null) client.Email = request.Email;
        if (request.Notes != null) client.Notes = request.Notes;
        if (request.Emirate != null)
            client.Emirate = Enum.Parse<Emirate>(request.Emirate, ignoreCase: true);

        client.UpdatedBy = _currentUser.UserId;
        client.UpdatedAt = _clock.UtcNow;

        await _db.SaveChangesAsync(ct);

        return Result<ClientDto>.Success(MapToDto(client, IncludeResolver.Parse(null)));
    }

    public async Task<Result<ClientDto>> VerifyAsync(Guid id, CancellationToken ct = default)
    {
        var client = await _db.Set<Client>().FindAsync([id], ct);

        if (client == null)
            return Result<ClientDto>.Failure("Client not found", "NOT_FOUND");

        if (client.IsVerified)
            return Result<ClientDto>.Failure("Client is already verified", "ALREADY_VERIFIED");

        client.IsVerified = true;
        client.VerifiedAt = _clock.UtcNow;
        client.VerifiedByUserId = _currentUser.UserId;
        client.UpdatedBy = _currentUser.UserId;
        client.UpdatedAt = _clock.UtcNow;

        await _db.SaveChangesAsync(ct);

        // Publish event
        await _publisher.Publish(new ClientVerifiedEvent
        {
            TenantId = _tenantContext.TenantId,
            ClientId = client.Id,
            VerifiedByUserId = _currentUser.UserId
        }, ct);

        _logger.LogInformation("Client {ClientId} verified by user {UserId}", 
            client.Id, _currentUser.UserId);

        return Result<ClientDto>.Success(MapToDto(client, IncludeResolver.Parse(null)));
    }

    public async Task<Result<ClientDto>> BlockAsync(Guid id, string reason, CancellationToken ct = default)
    {
        var client = await _db.Set<Client>().FindAsync([id], ct);

        if (client == null)
            return Result<ClientDto>.Failure("Client not found", "NOT_FOUND");

        if (client.SponsorFileStatus == SponsorFileStatus.Blocked)
            return Result<ClientDto>.Failure("Client is already blocked", "ALREADY_BLOCKED");

        client.SponsorFileStatus = SponsorFileStatus.Blocked;
        client.BlockedReason = reason;
        client.UpdatedBy = _currentUser.UserId;
        client.UpdatedAt = _clock.UtcNow;

        await _db.SaveChangesAsync(ct);

        // Publish event
        await _publisher.Publish(new ClientBlockedEvent
        {
            TenantId = _tenantContext.TenantId,
            ClientId = client.Id,
            Reason = reason,
            BlockedByUserId = _currentUser.UserId
        }, ct);

        _logger.LogInformation("Client {ClientId} blocked: {Reason}", client.Id, reason);

        return Result<ClientDto>.Success(MapToDto(client, IncludeResolver.Parse(null)));
    }

    public async Task<Result<ClientDto>> UnblockAsync(Guid id, CancellationToken ct = default)
    {
        var client = await _db.Set<Client>().FindAsync([id], ct);

        if (client == null)
            return Result<ClientDto>.Failure("Client not found", "NOT_FOUND");

        if (client.SponsorFileStatus != SponsorFileStatus.Blocked)
            return Result<ClientDto>.Failure("Client is not blocked", "NOT_BLOCKED");

        client.SponsorFileStatus = SponsorFileStatus.Active;
        client.BlockedReason = null;
        client.UpdatedBy = _currentUser.UserId;
        client.UpdatedAt = _clock.UtcNow;

        await _db.SaveChangesAsync(ct);

        return Result<ClientDto>.Success(MapToDto(client, IncludeResolver.Parse(null)));
    }

    public async Task<PagedList<ClientDto>> ListAsync(QueryParameters query, CancellationToken ct = default)
    {
        var dbQuery = _db.Set<Client>().AsQueryable();

        // Apply filters using extension methods
        dbQuery = dbQuery.ApplyFilters(query.Filters, GetFilterExpressions());

        // Apply sorting
        dbQuery = dbQuery.ApplySort(query.GetSortFields(), GetSortExpressions());

        // Get paged results
        var pagedResult = await dbQuery.ToPagedListAsync(query.Page, query.PageSize, ct);

        var includes = query.GetIncludes();
        return new PagedList<ClientDto>(
            pagedResult.Items.Select(c => MapToDto(c, includes)).ToList(),
            pagedResult.TotalCount,
            pagedResult.Page,
            pagedResult.PageSize
        );
    }

    public async Task<PagedList<ClientRefDto>> SearchAsync(string searchTerm, QueryParameters query, CancellationToken ct = default)
    {
        var dbQuery = _db.Set<Client>()
            .Where(c => EF.Functions.ILike(c.FullNameEn, $"%{searchTerm}%") ||
                        EF.Functions.ILike(c.FullNameAr, $"%{searchTerm}%") ||
                        EF.Functions.ILike(c.EmiratesId, $"%{searchTerm}%"));

        var pagedResult = await dbQuery
            .OrderByDescending(c => c.CreatedAt)
            .ToPagedListAsync(query.Page, query.PageSize, ct);

        return new PagedList<ClientRefDto>(
            pagedResult.Items.Select(MapToRefDto).ToList(),
            pagedResult.TotalCount,
            pagedResult.Page,
            pagedResult.PageSize
        );
    }

    #endregion

    #region Document Operations

    public async Task<Result<List<ClientDocumentDto>>> GetDocumentsAsync(Guid clientId, CancellationToken ct = default)
    {
        var clientExists = await _db.Set<Client>().AnyAsync(c => c.Id == clientId, ct);
        if (!clientExists)
            return Result<List<ClientDocumentDto>>.Failure("Client not found", "NOT_FOUND");

        var documents = await _db.Set<ClientDocument>()
            .Where(d => d.ClientId == clientId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync(ct);

        return Result<List<ClientDocumentDto>>.Success(documents.Select(MapDocumentToDto).ToList());
    }

    public async Task<Result<ClientDocumentDto>> AddDocumentAsync(Guid clientId, AddDocumentRequest request, CancellationToken ct = default)
    {
        var clientExists = await _db.Set<Client>().AnyAsync(c => c.Id == clientId, ct);
        if (!clientExists)
            return Result<ClientDocumentDto>.Failure("Client not found", "NOT_FOUND");

        var document = new ClientDocument
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId,
            ClientId = clientId,
            DocumentType = Enum.Parse<ClientDocumentType>(request.DocumentType, ignoreCase: true),
            FileUrl = request.FileUrl,
            FileName = request.FileName,
            ExpiresAt = request.ExpiresAt,
            UploadedAt = _clock.UtcNow,
            UploadedByUserId = _currentUser.UserId
        };

        _db.Set<ClientDocument>().Add(document);
        await _db.SaveChangesAsync(ct);

        return Result<ClientDocumentDto>.Success(MapDocumentToDto(document));
    }

    public async Task<Result<ClientDocumentDto>> VerifyDocumentAsync(Guid clientId, Guid documentId, CancellationToken ct = default)
    {
        var document = await _db.Set<ClientDocument>()
            .FirstOrDefaultAsync(d => d.Id == documentId && d.ClientId == clientId, ct);

        if (document == null)
            return Result<ClientDocumentDto>.Failure("Document not found", "NOT_FOUND");

        document.IsVerified = true;
        document.VerifiedAt = _clock.UtcNow;
        document.VerifiedByUserId = _currentUser.UserId;

        await _db.SaveChangesAsync(ct);

        return Result<ClientDocumentDto>.Success(MapDocumentToDto(document));
    }

    public async Task<Result> DeleteDocumentAsync(Guid clientId, Guid documentId, CancellationToken ct = default)
    {
        var document = await _db.Set<ClientDocument>()
            .FirstOrDefaultAsync(d => d.Id == documentId && d.ClientId == clientId, ct);

        if (document == null)
            return Result.Failure("Document not found", "NOT_FOUND");

        _db.Set<ClientDocument>().Remove(document);
        await _db.SaveChangesAsync(ct);

        return Result.Success();
    }

    #endregion

    #region Communication Log Operations

    public async Task<PagedList<CommunicationLogDto>> GetCommunicationsAsync(Guid clientId, QueryParameters query, CancellationToken ct = default)
    {
        var pagedResult = await _db.Set<ClientCommunicationLog>()
            .Where(c => c.ClientId == clientId)
            .OrderByDescending(c => c.OccurredAt)
            .ToPagedListAsync(query.Page, query.PageSize, ct);

        return new PagedList<CommunicationLogDto>(
            pagedResult.Items.Select(MapCommunicationToDto).ToList(),
            pagedResult.TotalCount,
            pagedResult.Page,
            pagedResult.PageSize
        );
    }

    public async Task<Result<CommunicationLogDto>> AddCommunicationAsync(Guid clientId, AddCommunicationRequest request, CancellationToken ct = default)
    {
        var clientExists = await _db.Set<Client>().AnyAsync(c => c.Id == clientId, ct);
        if (!clientExists)
            return Result<CommunicationLogDto>.Failure("Client not found", "NOT_FOUND");

        var log = new ClientCommunicationLog
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId,
            ClientId = clientId,
            Channel = Enum.Parse<CommunicationChannel>(request.Channel, ignoreCase: true),
            Direction = Enum.Parse<CommunicationDirection>(request.Direction, ignoreCase: true),
            Summary = request.Summary,
            LoggedByUserId = _currentUser.UserId,
            OccurredAt = _clock.UtcNow
        };

        _db.Set<ClientCommunicationLog>().Add(log);
        await _db.SaveChangesAsync(ct);

        return Result<CommunicationLogDto>.Success(MapCommunicationToDto(log));
    }

    #endregion

    #region Discount Card Operations

    public async Task<Result<List<DiscountCardDto>>> GetDiscountCardsAsync(Guid clientId, CancellationToken ct = default)
    {
        var clientExists = await _db.Set<Client>().AnyAsync(c => c.Id == clientId, ct);
        if (!clientExists)
            return Result<List<DiscountCardDto>>.Failure("Client not found", "NOT_FOUND");

        var cards = await _db.Set<DiscountCard>()
            .Where(c => c.ClientId == clientId)
            .ToListAsync(ct);

        return Result<List<DiscountCardDto>>.Success(cards.Select(MapDiscountCardToDto).ToList());
    }

    public async Task<Result<DiscountCardDto>> AddDiscountCardAsync(Guid clientId, AddDiscountCardRequest request, CancellationToken ct = default)
    {
        var clientExists = await _db.Set<Client>().AnyAsync(c => c.Id == clientId, ct);
        if (!clientExists)
            return Result<DiscountCardDto>.Failure("Client not found", "NOT_FOUND");

        var card = new DiscountCard
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId,
            ClientId = clientId,
            CardType = Enum.Parse<DiscountCardType>(request.CardType, ignoreCase: true),
            CardNumber = request.CardNumber,
            DiscountPercentage = request.DiscountPercentage,
            ValidUntil = request.ValidUntil
        };

        _db.Set<DiscountCard>().Add(card);
        await _db.SaveChangesAsync(ct);

        return Result<DiscountCardDto>.Success(MapDiscountCardToDto(card));
    }

    #endregion

    #region Mapping Methods

    private static ClientRefDto MapToRefDto(Client client) => new()
    {
        Id = client.Id,
        Name = client.FullNameEn,
        EmiratesId = client.EmiratesId,
        Category = client.Category.ToString().ToLowerInvariant()
    };

    private static ClientDto MapToDto(Client client, IncludeSet includes) => new()
    {
        Id = client.Id,
        EmiratesId = client.EmiratesId,
        FullNameEn = client.FullNameEn,
        FullNameAr = client.FullNameAr,
        PassportNumber = client.PassportNumber,
        Nationality = client.Nationality,
        Category = client.Category.ToString().ToLowerInvariant(),
        Phone = client.Phone,
        Email = client.Email,
        SponsorFileStatus = client.SponsorFileStatus.ToString().ToLowerInvariant(),
        Emirate = client.Emirate?.ToString().ToLowerInvariant(),
        IsVerified = client.IsVerified,
        VerifiedAt = client.VerifiedAt,
        BlockedReason = client.BlockedReason,
        Notes = client.Notes,
        CreatedAt = client.CreatedAt,
        // Rule 4: Collections - omit if not included, [] if included but empty
        Documents = includes.Has("documents") 
            ? client.Documents?.Select(MapDocumentToDto).ToList() ?? [] 
            : null,
        DiscountCards = includes.Has("discountCards")
            ? client.DiscountCards?.Select(MapDiscountCardToDto).ToList() ?? []
            : null
    };

    private static ClientDocumentDto MapDocumentToDto(ClientDocument doc) => new()
    {
        Id = doc.Id,
        DocumentType = doc.DocumentType.ToString(),
        FileUrl = doc.FileUrl,
        FileName = doc.FileName,
        ExpiresAt = doc.ExpiresAt,
        IsVerified = doc.IsVerified,
        UploadedAt = doc.UploadedAt
    };

    private static DiscountCardDto MapDiscountCardToDto(DiscountCard card) => new()
    {
        Id = card.Id,
        CardType = card.CardType.ToString(),
        CardNumber = card.CardNumber,
        DiscountPercentage = card.DiscountPercentage,
        ValidUntil = card.ValidUntil
    };

    private static CommunicationLogDto MapCommunicationToDto(ClientCommunicationLog log) => new()
    {
        Id = log.Id,
        Channel = log.Channel.ToString().ToLowerInvariant(),
        Direction = log.Direction.ToString().ToLowerInvariant(),
        Summary = log.Summary,
        LoggedBy = new UserRefDto 
        { 
            Id = log.LoggedByUserId, 
            Name = "User" // Would be resolved from Identity service in production
        },
        OccurredAt = log.OccurredAt
    };

    #endregion

    #region Filter/Sort Expression Builders

    private static Dictionary<string, System.Linq.Expressions.Expression<Func<Client, object>>> GetFilterExpressions() => new(StringComparer.OrdinalIgnoreCase)
    {
        ["category"] = x => x.Category.ToString().ToLower(),
        ["sponsorFileStatus"] = x => x.SponsorFileStatus.ToString().ToLower(),
        ["isVerified"] = x => x.IsVerified,
        ["nationality"] = x => x.Nationality,
        ["emirate"] = x => x.Emirate != null ? x.Emirate.ToString()!.ToLower() : "",
        ["createdAt"] = x => x.CreatedAt
    };

    private static Dictionary<string, System.Linq.Expressions.Expression<Func<Client, object>>> GetSortExpressions() => new(StringComparer.OrdinalIgnoreCase)
    {
        ["createdAt"] = x => x.CreatedAt,
        ["fullNameEn"] = x => x.FullNameEn,
        ["fullNameAr"] = x => x.FullNameAr,
        ["category"] = x => x.Category,
        ["nationality"] = x => x.Nationality
    };

    #endregion
}
