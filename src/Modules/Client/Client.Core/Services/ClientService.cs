using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Client.Contracts;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Client.Core.Services;

public class ClientService : IClientService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ClientService> _logger;

    private static readonly Dictionary<string, Expression<Func<Entities.Client, object>>> FilterableFields = new()
    {
        ["name"] = x => x.NameEn,
        ["city"] = x => x.City!,
        ["isActive"] = x => x.IsActive,
        ["nationalId"] = x => x.NationalId!,
    };

    private static readonly Dictionary<string, Expression<Func<Entities.Client, object>>> SortableFields = new()
    {
        ["name"] = x => x.NameEn,
        ["createdAt"] = x => x.CreatedAt,
        ["city"] = x => x.City!,
    };

    public ClientService(AppDbContext db, ILogger<ClientService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedList<ClientListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<Entities.Client>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId)
            .ApplyFilters(qp.Filters, FilterableFields)
            .ApplySort(qp.GetSortFields(), SortableFields);

        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var searchLower = qp.Search.ToLower();
            query = query.Where(x =>
                x.NameEn.ToLower().Contains(searchLower) ||
                (x.NameAr != null && x.NameAr.ToLower().Contains(searchLower)) ||
                (x.NationalId != null && x.NationalId.ToLower().Contains(searchLower)) ||
                (x.Phone != null && x.Phone.ToLower().Contains(searchLower)) ||
                (x.Email != null && x.Email.ToLower().Contains(searchLower)));
        }

        return await query
            .Select(x => MapToListDto(x))
            .ToPagedListAsync(qp, ct);
    }

    public async Task<Result<ClientDto>> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var client = await _db.Set<Entities.Client>()
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);

        if (client is null)
            return Result<ClientDto>.NotFound($"Client with ID {id} not found");

        return Result<ClientDto>.Success(MapToDto(client));
    }

    public async Task<Result<ClientDto>> CreateAsync(Guid tenantId, CreateClientRequest request, CancellationToken ct = default)
    {
        // Check duplicate name within tenant
        var existsByName = await _db.Set<Entities.Client>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.TenantId == tenantId && x.NameEn == request.NameEn, ct);

        if (existsByName)
            return Result<ClientDto>.Conflict($"Client with name '{request.NameEn}' already exists in this tenant");

        // Check duplicate NationalId within tenant
        if (!string.IsNullOrWhiteSpace(request.NationalId))
        {
            var existsByNationalId = await _db.Set<Entities.Client>()
                .IgnoreQueryFilters()
                .AnyAsync(x => x.TenantId == tenantId && x.NationalId == request.NationalId, ct);

            if (existsByNationalId)
                return Result<ClientDto>.Conflict($"Client with national ID '{request.NationalId}' already exists in this tenant");
        }

        var client = new Entities.Client
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            NationalId = request.NationalId,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address,
            City = request.City,
            Notes = request.Notes,
            IsActive = true,
        };

        _db.Set<Entities.Client>().Add(client);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created client {ClientId} ({NameEn}) for tenant {TenantId}", client.Id, client.NameEn, tenantId);

        return Result<ClientDto>.Success(MapToDto(client));
    }

    public async Task<Result<ClientDto>> UpdateAsync(Guid tenantId, Guid id, UpdateClientRequest request, CancellationToken ct = default)
    {
        var client = await _db.Set<Entities.Client>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);

        if (client is null)
            return Result<ClientDto>.NotFound($"Client with ID {id} not found");

        // Check duplicate name if being changed
        if (request.NameEn is not null && request.NameEn != client.NameEn)
        {
            var existsByName = await _db.Set<Entities.Client>()
                .IgnoreQueryFilters()
                .AnyAsync(x => x.TenantId == tenantId && x.NameEn == request.NameEn && x.Id != id, ct);

            if (existsByName)
                return Result<ClientDto>.Conflict($"Client with name '{request.NameEn}' already exists in this tenant");
        }

        // Check duplicate NationalId if being changed
        if (request.NationalId is not null && request.NationalId != client.NationalId)
        {
            if (!string.IsNullOrWhiteSpace(request.NationalId))
            {
                var existsByNationalId = await _db.Set<Entities.Client>()
                    .IgnoreQueryFilters()
                    .AnyAsync(x => x.TenantId == tenantId && x.NationalId == request.NationalId && x.Id != id, ct);

                if (existsByNationalId)
                    return Result<ClientDto>.Conflict($"Client with national ID '{request.NationalId}' already exists in this tenant");
            }
        }

        if (request.NameEn is not null) client.NameEn = request.NameEn;
        if (request.NameAr is not null) client.NameAr = request.NameAr;
        if (request.NationalId is not null) client.NationalId = request.NationalId;
        if (request.Phone is not null) client.Phone = request.Phone;
        if (request.Email is not null) client.Email = request.Email;
        if (request.Address is not null) client.Address = request.Address;
        if (request.City is not null) client.City = request.City;
        if (request.Notes is not null) client.Notes = request.Notes;
        if (request.IsActive.HasValue) client.IsActive = request.IsActive.Value;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated client {ClientId}", id);

        return Result<ClientDto>.Success(MapToDto(client));
    }

    public async Task<Result<bool>> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var client = await _db.Set<Entities.Client>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);

        if (client is null)
            return Result<bool>.NotFound($"Client with ID {id} not found");

        client.IsActive = false;
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Deactivated client {ClientId}", id);

        return Result<bool>.Success(true);
    }

    private static ClientDto MapToDto(Entities.Client c) => new()
    {
        Id = c.Id,
        NameEn = c.NameEn,
        NameAr = c.NameAr,
        NationalId = c.NationalId,
        Phone = c.Phone,
        Email = c.Email,
        Address = c.Address,
        City = c.City,
        Notes = c.Notes,
        IsActive = c.IsActive,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt,
    };

    private static ClientListDto MapToListDto(Entities.Client c) => new()
    {
        Id = c.Id,
        NameEn = c.NameEn,
        NameAr = c.NameAr,
        NationalId = c.NationalId,
        Phone = c.Phone,
        Email = c.Email,
        City = c.City,
        IsActive = c.IsActive,
        CreatedAt = c.CreatedAt,
    };
}
