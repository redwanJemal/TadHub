using ClientManagement.Contracts;
using ClientManagement.Contracts.DTOs;
using ClientManagement.Core.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;

namespace ClientManagement.Core.Services;

/// <summary>
/// Service implementation for lead management.
/// </summary>
public class LeadService : ILeadService
{
    private readonly AppDbContext _db;
    private readonly IClientService _clientService;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly IClock _clock;
    private readonly ILogger<LeadService> _logger;

    public LeadService(
        AppDbContext db,
        IClientService clientService,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        IClock clock,
        ILogger<LeadService> logger)
    {
        _db = db;
        _clientService = clientService;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _clock = clock;
        _logger = logger;
    }

    public async Task<Result<LeadDto>> CreateAsync(CreateLeadRequest request, CancellationToken ct = default)
    {
        var lead = new Lead
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantContext.TenantId,
            Source = Enum.Parse<LeadSource>(request.Source, ignoreCase: true),
            Status = LeadStatus.New,
            ContactName = request.ContactName,
            ContactPhone = request.ContactPhone,
            ContactEmail = request.ContactEmail,
            Notes = request.Notes,
            AssignedToUserId = request.AssignedToUserId,
            CreatedBy = _currentUser.UserId
        };

        _db.Set<Lead>().Add(lead);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Lead {LeadId} created from source {Source}", lead.Id, lead.Source);

        return Result<LeadDto>.Success(MapToDto(lead, IncludeResolver.Parse(null)));
    }

    public async Task<Result<LeadDto>> GetByIdAsync(Guid id, IncludeSet includes, CancellationToken ct = default)
    {
        var query = _db.Set<Lead>().AsQueryable();

        if (includes.Has("client"))
            query = query.Include(l => l.Client);

        var lead = await query.FirstOrDefaultAsync(l => l.Id == id, ct);

        if (lead == null)
            return Result<LeadDto>.Failure("Lead not found", "NOT_FOUND");

        return Result<LeadDto>.Success(MapToDto(lead, includes));
    }

    public async Task<Result<LeadDto>> UpdateAsync(Guid id, UpdateLeadRequest request, CancellationToken ct = default)
    {
        var lead = await _db.Set<Lead>().FindAsync([id], ct);

        if (lead == null)
            return Result<LeadDto>.Failure("Lead not found", "NOT_FOUND");

        if (request.Status != null)
            lead.Status = Enum.Parse<LeadStatus>(request.Status, ignoreCase: true);
        if (request.Notes != null)
            lead.Notes = request.Notes;
        if (request.AssignedToUserId != null)
            lead.AssignedToUserId = request.AssignedToUserId;
        if (request.ContactName != null)
            lead.ContactName = request.ContactName;
        if (request.ContactPhone != null)
            lead.ContactPhone = request.ContactPhone;
        if (request.ContactEmail != null)
            lead.ContactEmail = request.ContactEmail;

        lead.UpdatedBy = _currentUser.UserId;
        lead.UpdatedAt = _clock.UtcNow;

        await _db.SaveChangesAsync(ct);

        return Result<LeadDto>.Success(MapToDto(lead, IncludeResolver.Parse(null)));
    }

    public async Task<Result<ClientDto>> ConvertToClientAsync(Guid leadId, ConvertLeadRequest request, CancellationToken ct = default)
    {
        var lead = await _db.Set<Lead>().FindAsync([leadId], ct);

        if (lead == null)
            return Result<ClientDto>.Failure("Lead not found", "NOT_FOUND");

        if (lead.Status == LeadStatus.Converted)
            return Result<ClientDto>.Failure("Lead is already converted", "ALREADY_CONVERTED");

        // Create the client
        var clientResult = await _clientService.RegisterAsync(request.Client, ct);

        if (!clientResult.IsSuccess)
            return clientResult;

        // Link lead to client
        lead.ClientId = clientResult.Value!.Id;
        lead.Status = LeadStatus.Converted;
        lead.ConvertedAt = _clock.UtcNow;
        lead.UpdatedBy = _currentUser.UserId;
        lead.UpdatedAt = _clock.UtcNow;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Lead {LeadId} converted to client {ClientId}", leadId, clientResult.Value.Id);

        return clientResult;
    }

    public async Task<PagedList<LeadDto>> ListAsync(QueryParameters query, CancellationToken ct = default)
    {
        var dbQuery = _db.Set<Lead>().AsQueryable();

        // Apply filters
        dbQuery = dbQuery.ApplyFilters(query.Filters, GetFilterExpressions());

        // Apply sorting
        dbQuery = dbQuery.ApplySort(query.GetSortFields(), GetSortExpressions());

        var pagedResult = await dbQuery.ToPagedListAsync(query.Page, query.PageSize, ct);

        var includes = query.GetIncludes();
        return new PagedList<LeadDto>(
            pagedResult.Items.Select(l => MapToDto(l, includes)).ToList(),
            pagedResult.TotalCount,
            pagedResult.Page,
            pagedResult.PageSize
        );
    }

    public async Task<LeadFunnelStats> GetFunnelStatsAsync(DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default)
    {
        var query = _db.Set<Lead>().AsQueryable();

        if (from.HasValue)
            query = query.Where(l => l.CreatedAt >= from.Value);
        if (to.HasValue)
            query = query.Where(l => l.CreatedAt <= to.Value);

        var stats = await query
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                New = g.Count(l => l.Status == LeadStatus.New),
                Contacted = g.Count(l => l.Status == LeadStatus.Contacted),
                Qualified = g.Count(l => l.Status == LeadStatus.Qualified),
                Converted = g.Count(l => l.Status == LeadStatus.Converted),
                Lost = g.Count(l => l.Status == LeadStatus.Lost)
            })
            .FirstOrDefaultAsync(ct);

        if (stats == null)
            return new LeadFunnelStats();

        return new LeadFunnelStats
        {
            TotalLeads = stats.Total,
            NewLeads = stats.New,
            ContactedLeads = stats.Contacted,
            QualifiedLeads = stats.Qualified,
            ConvertedLeads = stats.Converted,
            LostLeads = stats.Lost,
            ConversionRate = stats.Total > 0 ? (decimal)stats.Converted / stats.Total * 100 : 0
        };
    }

    #region Mapping

    private static LeadDto MapToDto(Lead lead, IncludeSet includes) => new()
    {
        Id = lead.Id,
        // Rule 2: RefDto when not included, full object when included
        Client = lead.Client != null 
            ? new ClientRefDto
            {
                Id = lead.Client.Id,
                Name = lead.Client.FullNameEn,
                EmiratesId = lead.Client.EmiratesId,
                Category = lead.Client.Category.ToString().ToLowerInvariant()
            }
            : null, // Rule 3: Nullable relations are explicitly null
        Source = lead.Source.ToString().ToLowerInvariant(),
        Status = lead.Status.ToString().ToLowerInvariant(),
        Notes = lead.Notes,
        AssignedTo = lead.AssignedToUserId.HasValue 
            ? new UserRefDto { Id = lead.AssignedToUserId.Value, Name = "User" }
            : null,
        ContactName = lead.ContactName,
        ContactPhone = lead.ContactPhone,
        ContactEmail = lead.ContactEmail,
        CreatedAt = lead.CreatedAt,
        UpdatedAt = lead.UpdatedAt
    };

    private static Dictionary<string, System.Linq.Expressions.Expression<Func<Lead, object>>> GetFilterExpressions() => new(StringComparer.OrdinalIgnoreCase)
    {
        ["status"] = x => x.Status.ToString().ToLower(),
        ["source"] = x => x.Source.ToString().ToLower(),
        ["assignedToUserId"] = x => x.AssignedToUserId ?? Guid.Empty,
        ["createdAt"] = x => x.CreatedAt
    };

    private static Dictionary<string, System.Linq.Expressions.Expression<Func<Lead, object>>> GetSortExpressions() => new(StringComparer.OrdinalIgnoreCase)
    {
        ["createdAt"] = x => x.CreatedAt,
        ["status"] = x => x.Status,
        ["source"] = x => x.Source
    };

    #endregion
}
