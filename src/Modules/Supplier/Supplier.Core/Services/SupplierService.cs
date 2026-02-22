using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Supplier.Contracts;
using Supplier.Contracts.DTOs;
using Supplier.Core.Entities;
using TadHub.Infrastructure.Api;
using TadHub.Infrastructure.Persistence;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Interfaces;
using TadHub.SharedKernel.Models;

namespace Supplier.Core.Services;

/// <summary>
/// Service for managing suppliers, contacts, and tenant-supplier relationships.
/// </summary>
public class SupplierService : ISupplierService
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;
    private readonly ILogger<SupplierService> _logger;

    /// <summary>
    /// Fields available for filtering supplier list queries.
    /// </summary>
    private static readonly Dictionary<string, Expression<Func<Entities.Supplier, object>>> SupplierFilterableFields = new()
    {
        ["nameEn"] = x => x.NameEn,
        ["country"] = x => x.Country,
        ["city"] = x => x.City!,
        ["isActive"] = x => x.IsActive,
        ["licenseNumber"] = x => x.LicenseNumber!,
        ["createdAt"] = x => x.CreatedAt
    };

    /// <summary>
    /// Fields available for sorting supplier list queries.
    /// </summary>
    private static readonly Dictionary<string, Expression<Func<Entities.Supplier, object>>> SupplierSortableFields = new()
    {
        ["nameEn"] = x => x.NameEn,
        ["country"] = x => x.Country,
        ["createdAt"] = x => x.CreatedAt,
        ["updatedAt"] = x => x.UpdatedAt
    };

    /// <summary>
    /// Fields available for filtering tenant-supplier list queries.
    /// </summary>
    private static readonly Dictionary<string, Expression<Func<TenantSupplier, object>>> TenantSupplierFilterableFields = new()
    {
        ["status"] = x => x.Status,
        ["supplierId"] = x => x.SupplierId,
        ["createdAt"] = x => x.CreatedAt
    };

    /// <summary>
    /// Fields available for sorting tenant-supplier list queries.
    /// </summary>
    private static readonly Dictionary<string, Expression<Func<TenantSupplier, object>>> TenantSupplierSortableFields = new()
    {
        ["status"] = x => x.Status,
        ["createdAt"] = x => x.CreatedAt,
        ["updatedAt"] = x => x.UpdatedAt,
        ["agreementStartDate"] = x => x.AgreementStartDate!,
        ["agreementEndDate"] = x => x.AgreementEndDate!
    };

    public SupplierService(
        AppDbContext db,
        IClock clock,
        ILogger<SupplierService> logger)
    {
        _db = db;
        _clock = clock;
        _logger = logger;
    }

    #region Supplier CRUD

    public async Task<Result<SupplierDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var supplier = await _db.Set<Entities.Supplier>()
            .AsNoTracking()
            .Include(x => x.Contacts.Where(c => c.IsActive))
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (supplier is null)
            return Result<SupplierDto>.NotFound($"Supplier with ID {id} not found");

        return Result<SupplierDto>.Success(MapToDto(supplier, includeContacts: true));
    }

    public async Task<PagedList<SupplierDto>> ListAsync(QueryParameters qp, CancellationToken ct = default)
    {
        var query = _db.Set<Entities.Supplier>()
            .AsNoTracking()
            .ApplyFilters(qp.Filters, SupplierFilterableFields)
            .ApplySort(qp.GetSortFields(), SupplierSortableFields);

        // Apply search if provided
        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var searchLower = qp.Search.ToLower();
            query = query.Where(x =>
                x.NameEn.ToLower().Contains(searchLower) ||
                (x.NameAr != null && x.NameAr.ToLower().Contains(searchLower)) ||
                (x.Email != null && x.Email.ToLower().Contains(searchLower)) ||
                (x.LicenseNumber != null && x.LicenseNumber.ToLower().Contains(searchLower)));
        }

        var pagedList = await query
            .Select(x => MapToDto(x, false))
            .ToPagedListAsync(qp, ct);

        return pagedList;
    }

    public async Task<Result<SupplierDto>> CreateAsync(CreateSupplierRequest request, CancellationToken ct = default)
    {
        // Check for duplicate license number if provided
        if (!string.IsNullOrWhiteSpace(request.LicenseNumber))
        {
            var existsByLicense = await _db.Set<Entities.Supplier>()
                .AnyAsync(x => x.LicenseNumber == request.LicenseNumber, ct);
            if (existsByLicense)
                return Result<SupplierDto>.Conflict($"Supplier with license number '{request.LicenseNumber}' already exists");
        }

        var supplier = new Entities.Supplier
        {
            Id = Guid.NewGuid(),
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            Country = request.Country,
            City = request.City,
            LicenseNumber = request.LicenseNumber,
            Phone = request.Phone,
            Email = request.Email,
            Website = request.Website,
            Notes = request.Notes,
            IsActive = true
        };

        _db.Set<Entities.Supplier>().Add(supplier);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created supplier {SupplierId} ({NameEn})", supplier.Id, supplier.NameEn);

        return Result<SupplierDto>.Success(MapToDto(supplier, includeContacts: false));
    }

    public async Task<Result<SupplierDto>> UpdateAsync(Guid id, UpdateSupplierRequest request, CancellationToken ct = default)
    {
        var supplier = await _db.Set<Entities.Supplier>().FindAsync([id], ct);
        if (supplier is null)
            return Result<SupplierDto>.NotFound($"Supplier with ID {id} not found");

        // Check for duplicate license number if being changed
        if (request.LicenseNumber is not null && request.LicenseNumber != supplier.LicenseNumber)
        {
            if (!string.IsNullOrWhiteSpace(request.LicenseNumber))
            {
                var existsByLicense = await _db.Set<Entities.Supplier>()
                    .AnyAsync(x => x.LicenseNumber == request.LicenseNumber && x.Id != id, ct);
                if (existsByLicense)
                    return Result<SupplierDto>.Conflict($"Supplier with license number '{request.LicenseNumber}' already exists");
            }
        }

        // Apply updates (only non-null values)
        if (request.NameEn is not null)
            supplier.NameEn = request.NameEn;
        if (request.NameAr is not null)
            supplier.NameAr = request.NameAr;
        if (request.Country is not null)
            supplier.Country = request.Country;
        if (request.City is not null)
            supplier.City = request.City;
        if (request.LicenseNumber is not null)
            supplier.LicenseNumber = request.LicenseNumber;
        if (request.Phone is not null)
            supplier.Phone = request.Phone;
        if (request.Email is not null)
            supplier.Email = request.Email;
        if (request.Website is not null)
            supplier.Website = request.Website;
        if (request.Notes is not null)
            supplier.Notes = request.Notes;
        if (request.IsActive.HasValue)
            supplier.IsActive = request.IsActive.Value;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated supplier {SupplierId}", supplier.Id);

        return Result<SupplierDto>.Success(MapToDto(supplier, includeContacts: false));
    }

    #endregion

    #region Supplier Contacts

    public async Task<Result<List<SupplierContactDto>>> GetContactsAsync(Guid supplierId, CancellationToken ct = default)
    {
        var supplierExists = await _db.Set<Entities.Supplier>()
            .AnyAsync(x => x.Id == supplierId, ct);

        if (!supplierExists)
            return Result<List<SupplierContactDto>>.NotFound($"Supplier with ID {supplierId} not found");

        var contacts = await _db.Set<SupplierContact>()
            .AsNoTracking()
            .Where(x => x.SupplierId == supplierId)
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.FullName)
            .Select(x => MapContactToDto(x))
            .ToListAsync(ct);

        return Result<List<SupplierContactDto>>.Success(contacts);
    }

    public async Task<Result<SupplierContactDto>> AddContactAsync(Guid supplierId, CreateSupplierContactRequest request, CancellationToken ct = default)
    {
        var supplierExists = await _db.Set<Entities.Supplier>()
            .AnyAsync(x => x.Id == supplierId, ct);

        if (!supplierExists)
            return Result<SupplierContactDto>.NotFound($"Supplier with ID {supplierId} not found");

        // Check for duplicate email within the same supplier
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var existsByEmail = await _db.Set<SupplierContact>()
                .AnyAsync(x => x.SupplierId == supplierId && x.Email == request.Email, ct);
            if (existsByEmail)
                return Result<SupplierContactDto>.Conflict($"Contact with email '{request.Email}' already exists for this supplier");
        }

        // If marking as primary, unmark existing primary contacts
        if (request.IsPrimary)
        {
            var existingPrimary = await _db.Set<SupplierContact>()
                .Where(x => x.SupplierId == supplierId && x.IsPrimary)
                .ToListAsync(ct);

            foreach (var existing in existingPrimary)
            {
                existing.IsPrimary = false;
            }
        }

        var contact = new SupplierContact
        {
            Id = Guid.NewGuid(),
            SupplierId = supplierId,
            UserId = request.UserId,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            JobTitle = request.JobTitle,
            IsPrimary = request.IsPrimary,
            IsActive = true
        };

        _db.Set<SupplierContact>().Add(contact);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Added contact {ContactId} to supplier {SupplierId}", contact.Id, supplierId);

        return Result<SupplierContactDto>.Success(MapContactToDto(contact));
    }

    public async Task<Result> RemoveContactAsync(Guid supplierId, Guid contactId, CancellationToken ct = default)
    {
        var contact = await _db.Set<SupplierContact>()
            .FirstOrDefaultAsync(x => x.Id == contactId && x.SupplierId == supplierId, ct);

        if (contact is null)
            return Result.NotFound($"Contact with ID {contactId} not found for supplier {supplierId}");

        _db.Set<SupplierContact>().Remove(contact);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Removed contact {ContactId} from supplier {SupplierId}", contactId, supplierId);

        return Result.Success();
    }

    #endregion

    #region Tenant-Supplier Relationships

    public async Task<Result<TenantSupplierDto>> GetTenantSupplierByIdAsync(Guid tenantId, Guid id, QueryParameters? qp = null, CancellationToken ct = default)
    {
        var includes = qp?.GetIncludeList() ?? [];
        var includeSupplier = includes.Contains("supplier", StringComparer.OrdinalIgnoreCase);

        var query = _db.Set<TenantSupplier>()
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId);

        if (includeSupplier)
        {
            query = query.Include(x => x.Supplier);
        }

        var tenantSupplier = await query.FirstOrDefaultAsync(x => x.Id == id, ct);

        if (tenantSupplier is null)
            return Result<TenantSupplierDto>.NotFound($"Tenant-supplier relationship with ID {id} not found");

        return Result<TenantSupplierDto>.Success(MapTenantSupplierToDto(tenantSupplier, includeSupplier));
    }

    public async Task<PagedList<TenantSupplierDto>> ListTenantSuppliersAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default)
    {
        var includes = qp.GetIncludeList();
        var includeSupplier = includes.Contains("supplier", StringComparer.OrdinalIgnoreCase);

        var query = _db.Set<TenantSupplier>()
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(x => x.TenantId == tenantId);

        if (includeSupplier)
        {
            query = query.Include(x => x.Supplier);
        }

        query = query
            .ApplyFilters(qp.Filters, TenantSupplierFilterableFields)
            .ApplySort(qp.GetSortFields(), TenantSupplierSortableFields);

        // Apply search if provided (searches on supplier name)
        if (!string.IsNullOrWhiteSpace(qp.Search))
        {
            var searchLower = qp.Search.ToLower();
            query = query
                .Include(x => x.Supplier)
                .Where(x =>
                    x.Supplier.NameEn.ToLower().Contains(searchLower) ||
                    (x.Supplier.NameAr != null && x.Supplier.NameAr.ToLower().Contains(searchLower)) ||
                    (x.ContractReference != null && x.ContractReference.ToLower().Contains(searchLower)));
        }

        var pagedList = await query
            .Select(x => MapTenantSupplierToDto(x, includeSupplier))
            .ToPagedListAsync(qp, ct);

        return pagedList;
    }

    public async Task<Result<TenantSupplierDto>> LinkSupplierToTenantAsync(Guid tenantId, LinkSupplierRequest request, CancellationToken ct = default)
    {
        // Check that the supplier exists
        var supplier = await _db.Set<Entities.Supplier>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.SupplierId, ct);

        if (supplier is null)
            return Result<TenantSupplierDto>.NotFound($"Supplier with ID {request.SupplierId} not found");

        if (!supplier.IsActive)
            return Result<TenantSupplierDto>.ValidationError("Cannot link to an inactive supplier");

        // Check for duplicate relationship
        var existingLink = await _db.Set<TenantSupplier>()
            .IgnoreQueryFilters()
            .AnyAsync(x => x.TenantId == tenantId && x.SupplierId == request.SupplierId, ct);

        if (existingLink)
            return Result<TenantSupplierDto>.Conflict($"Supplier is already linked to this tenant");

        var tenantSupplier = new TenantSupplier
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            SupplierId = request.SupplierId,
            Status = SupplierRelationshipStatus.Active,
            ContractReference = request.ContractReference,
            Notes = request.Notes,
            AgreementStartDate = request.AgreementStartDate,
            AgreementEndDate = request.AgreementEndDate
        };

        _db.Set<TenantSupplier>().Add(tenantSupplier);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Linked supplier {SupplierId} to tenant {TenantId}", request.SupplierId, tenantId);

        // Load the supplier for the response
        tenantSupplier.Supplier = supplier;
        return Result<TenantSupplierDto>.Success(MapTenantSupplierToDto(tenantSupplier, includeSupplier: true));
    }

    public async Task<Result<TenantSupplierDto>> UpdateTenantSupplierAsync(Guid tenantId, Guid id, UpdateTenantSupplierRequest request, CancellationToken ct = default)
    {
        var tenantSupplier = await _db.Set<TenantSupplier>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);

        if (tenantSupplier is null)
            return Result<TenantSupplierDto>.NotFound($"Tenant-supplier relationship with ID {id} not found");

        // Apply updates (only non-null values)
        if (request.Status is not null)
        {
            if (!Enum.TryParse<SupplierRelationshipStatus>(request.Status, ignoreCase: true, out var status))
                return Result<TenantSupplierDto>.ValidationError($"Invalid status '{request.Status}'. Valid values: Active, Suspended, Terminated");
            tenantSupplier.Status = status;
        }

        if (request.ContractReference is not null)
            tenantSupplier.ContractReference = request.ContractReference;
        if (request.Notes is not null)
            tenantSupplier.Notes = request.Notes;
        if (request.AgreementStartDate.HasValue)
            tenantSupplier.AgreementStartDate = request.AgreementStartDate;
        if (request.AgreementEndDate.HasValue)
            tenantSupplier.AgreementEndDate = request.AgreementEndDate;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Updated tenant-supplier relationship {TenantSupplierId}", id);

        return Result<TenantSupplierDto>.Success(MapTenantSupplierToDto(tenantSupplier, includeSupplier: false));
    }

    public async Task<Result> UnlinkSupplierFromTenantAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var tenantSupplier = await _db.Set<TenantSupplier>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);

        if (tenantSupplier is null)
            return Result.NotFound($"Tenant-supplier relationship with ID {id} not found");

        _db.Set<TenantSupplier>().Remove(tenantSupplier);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Unlinked supplier {SupplierId} from tenant {TenantId}", tenantSupplier.SupplierId, tenantId);

        return Result.Success();
    }

    #endregion

    #region Mapping

    private static SupplierDto MapToDto(Entities.Supplier supplier, bool includeContacts)
    {
        return new SupplierDto
        {
            Id = supplier.Id,
            NameEn = supplier.NameEn,
            NameAr = supplier.NameAr,
            Country = supplier.Country,
            City = supplier.City,
            LicenseNumber = supplier.LicenseNumber,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Website = supplier.Website,
            Notes = supplier.Notes,
            IsActive = supplier.IsActive,
            CreatedAt = supplier.CreatedAt,
            UpdatedAt = supplier.UpdatedAt,
            Contacts = includeContacts
                ? supplier.Contacts.Select(MapContactToDto).ToList()
                : null
        };
    }

    private static SupplierContactDto MapContactToDto(SupplierContact contact)
    {
        return new SupplierContactDto
        {
            Id = contact.Id,
            SupplierId = contact.SupplierId,
            UserId = contact.UserId,
            FullName = contact.FullName,
            Email = contact.Email,
            Phone = contact.Phone,
            JobTitle = contact.JobTitle,
            IsPrimary = contact.IsPrimary,
            IsActive = contact.IsActive,
            CreatedAt = contact.CreatedAt,
            UpdatedAt = contact.UpdatedAt
        };
    }

    private static TenantSupplierDto MapTenantSupplierToDto(TenantSupplier tenantSupplier, bool includeSupplier)
    {
        return new TenantSupplierDto
        {
            Id = tenantSupplier.Id,
            TenantId = tenantSupplier.TenantId,
            SupplierId = tenantSupplier.SupplierId,
            Status = tenantSupplier.Status.ToString(),
            ContractReference = tenantSupplier.ContractReference,
            Notes = tenantSupplier.Notes,
            AgreementStartDate = tenantSupplier.AgreementStartDate,
            AgreementEndDate = tenantSupplier.AgreementEndDate,
            CreatedAt = tenantSupplier.CreatedAt,
            UpdatedAt = tenantSupplier.UpdatedAt,
            Supplier = includeSupplier && tenantSupplier.Supplier is not null
                ? MapToDto(tenantSupplier.Supplier, includeContacts: false)
                : null
        };
    }

    #endregion
}
