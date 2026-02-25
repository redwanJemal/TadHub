using Supplier.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Supplier.Contracts;

/// <summary>
/// Service for managing suppliers, contacts, and tenant-supplier relationships.
/// </summary>
public interface ISupplierService
{
    #region Supplier CRUD (Platform-level)

    /// <summary>
    /// Gets a supplier by ID.
    /// </summary>
    /// <param name="id">The supplier ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The supplier if found.</returns>
    Task<Result<SupplierDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Lists suppliers with filtering, sorting, and pagination.
    /// </summary>
    /// <param name="qp">Query parameters (filters, sort, pagination).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paged list of suppliers.</returns>
    Task<PagedList<SupplierDto>> ListAsync(QueryParameters qp, CancellationToken ct = default);

    /// <summary>
    /// Creates a new global supplier.
    /// </summary>
    /// <param name="request">The creation request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created supplier.</returns>
    Task<Result<SupplierDto>> CreateAsync(CreateSupplierRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates an existing supplier.
    /// </summary>
    /// <param name="id">The supplier ID.</param>
    /// <param name="request">The update request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated supplier.</returns>
    Task<Result<SupplierDto>> UpdateAsync(Guid id, UpdateSupplierRequest request, CancellationToken ct = default);

    #endregion

    #region Supplier Contacts

    /// <summary>
    /// Gets contacts for a supplier.
    /// </summary>
    /// <param name="supplierId">The supplier ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of contacts.</returns>
    Task<Result<List<SupplierContactDto>>> GetContactsAsync(Guid supplierId, CancellationToken ct = default);

    /// <summary>
    /// Adds a contact to a supplier.
    /// </summary>
    /// <param name="supplierId">The supplier ID.</param>
    /// <param name="request">The contact creation request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created contact.</returns>
    Task<Result<SupplierContactDto>> AddContactAsync(Guid supplierId, CreateSupplierContactRequest request, CancellationToken ct = default);

    /// <summary>
    /// Removes a contact from a supplier.
    /// </summary>
    /// <param name="supplierId">The supplier ID.</param>
    /// <param name="contactId">The contact ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success or failure.</returns>
    Task<Result> RemoveContactAsync(Guid supplierId, Guid contactId, CancellationToken ct = default);

    #endregion

    #region Tenant-Supplier Relationships

    /// <summary>
    /// Gets a tenant-supplier relationship by ID.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="id">The tenant-supplier relationship ID.</param>
    /// <param name="qp">Query parameters (for includes).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The tenant-supplier relationship if found.</returns>
    Task<Result<TenantSupplierDto>> GetTenantSupplierByIdAsync(Guid tenantId, Guid id, QueryParameters? qp = null, CancellationToken ct = default);

    /// <summary>
    /// Lists suppliers linked to a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="qp">Query parameters (filters, sort, pagination).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paged list of tenant-supplier relationships.</returns>
    Task<PagedList<TenantSupplierDto>> ListTenantSuppliersAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);

    /// <summary>
    /// Creates a new supplier and immediately links it to the tenant.
    /// </summary>
    Task<Result<TenantSupplierDto>> CreateAndLinkAsync(Guid tenantId, CreateSupplierRequest request, CancellationToken ct = default);

    /// <summary>
    /// Links a supplier to a tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="request">The link request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created tenant-supplier relationship.</returns>
    Task<Result<TenantSupplierDto>> LinkSupplierToTenantAsync(Guid tenantId, LinkSupplierRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates a tenant-supplier relationship.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="id">The tenant-supplier relationship ID.</param>
    /// <param name="request">The update request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The updated tenant-supplier relationship.</returns>
    Task<Result<TenantSupplierDto>> UpdateTenantSupplierAsync(Guid tenantId, Guid id, UpdateTenantSupplierRequest request, CancellationToken ct = default);

    /// <summary>
    /// Unlinks a supplier from a tenant (deletes the relationship).
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <param name="id">The tenant-supplier relationship ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success or failure.</returns>
    Task<Result> UnlinkSupplierFromTenantAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    #endregion
}
