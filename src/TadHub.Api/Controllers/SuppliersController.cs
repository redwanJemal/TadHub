using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Supplier.Contracts;
using Supplier.Contracts.DTOs;
using TadHub.Api.Filters;
using TadHub.Infrastructure.Auth;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace TadHub.Api.Controllers;

#region Platform Admin Endpoints

/// <summary>
/// Platform admin supplier management endpoints.
/// These endpoints allow platform admins to manage the global supplier catalog.
/// </summary>
[ApiController]
[Route("api/v1/suppliers")]
[Authorize(Roles = "platform-admin")]
public class SuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    /// <summary>
    /// Lists all suppliers with filtering, sorting, and pagination.
    /// </summary>
    /// <remarks>
    /// Filters:
    /// - filter[country]=AE,PH (ISO alpha-2 codes)
    /// - filter[isActive]=true
    /// - filter[nameEn]=Acme
    ///
    /// Sort:
    /// - sort=-createdAt (default, newest first)
    /// - sort=nameEn
    ///
    /// Search:
    /// - search=acme (searches name, email, license number)
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PagedList<SupplierDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] QueryParameters qp, CancellationToken ct)
    {
        var result = await _supplierService.ListAsync(qp, ct);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new global supplier.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSupplierRequest request,
        CancellationToken ct)
    {
        var result = await _supplierService.CreateAsync(request, ct);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "CONFLICT" => Conflict(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error })
            };
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value!.Id },
            result.Value);
    }

    /// <summary>
    /// Gets a supplier by ID, including its contacts.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _supplierService.GetByIdAsync(id, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Updates a supplier (partial update).
    /// </summary>
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateSupplierRequest request,
        CancellationToken ct)
    {
        var result = await _supplierService.UpdateAsync(id, request, ct);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error }),
                "CONFLICT" => Conflict(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error })
            };
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets contacts for a supplier.
    /// </summary>
    [HttpGet("{supplierId:guid}/contacts")]
    [ProducesResponseType(typeof(List<SupplierContactDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContacts(Guid supplierId, CancellationToken ct)
    {
        var result = await _supplierService.GetContactsAsync(supplierId, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Adds a contact to a supplier.
    /// </summary>
    [HttpPost("{supplierId:guid}/contacts")]
    [ProducesResponseType(typeof(SupplierContactDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddContact(
        Guid supplierId,
        [FromBody] CreateSupplierContactRequest request,
        CancellationToken ct)
    {
        var result = await _supplierService.AddContactAsync(supplierId, request, ct);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error }),
                "CONFLICT" => Conflict(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error })
            };
        }

        return Created(
            $"/api/v1/suppliers/{supplierId}/contacts/{result.Value!.Id}",
            result.Value);
    }

    /// <summary>
    /// Removes a contact from a supplier.
    /// </summary>
    [HttpDelete("{supplierId:guid}/contacts/{contactId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveContact(
        Guid supplierId,
        Guid contactId,
        CancellationToken ct)
    {
        var result = await _supplierService.RemoveContactAsync(supplierId, contactId, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }
}

#endregion

#region Tenant-Scoped Endpoints

/// <summary>
/// Tenant-scoped supplier management endpoints.
/// These endpoints allow tenant staff to manage their supplier relationships.
/// </summary>
[ApiController]
[Route("api/v1/tenants/{tenantId:guid}/suppliers")]
[Authorize]
[TenantMemberRequired(TenantIdParameter = "tenantId")]
public class TenantSuppliersController : ControllerBase
{
    private readonly ISupplierService _supplierService;

    public TenantSuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    /// <summary>
    /// Lists suppliers linked to this tenant.
    /// </summary>
    /// <remarks>
    /// Filters:
    /// - filter[status]=Active,Suspended
    ///
    /// Sort:
    /// - sort=-createdAt (default)
    /// - sort=agreementStartDate
    ///
    /// Include:
    /// - include=supplier (includes full supplier details)
    ///
    /// Search:
    /// - search=acme (searches supplier name, contract reference)
    /// </remarks>
    [HttpGet]
    [HasPermission("supplier.view")]
    [ProducesResponseType(typeof(PagedList<TenantSupplierDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        Guid tenantId,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _supplierService.ListTenantSuppliersAsync(tenantId, qp, ct);
        return Ok(result);
    }

    /// <summary>
    /// Links a supplier to this tenant.
    /// </summary>
    [HttpPost]
    [HasPermission("supplier.manage")]
    [ProducesResponseType(typeof(TenantSupplierDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> LinkSupplier(
        Guid tenantId,
        [FromBody] LinkSupplierRequest request,
        CancellationToken ct)
    {
        var result = await _supplierService.LinkSupplierToTenantAsync(tenantId, request, ct);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error }),
                "CONFLICT" => Conflict(new { error = result.Error }),
                "VALIDATION_ERROR" => BadRequest(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error })
            };
        }

        return CreatedAtAction(
            nameof(GetById),
            new { tenantId, id = result.Value!.Id },
            result.Value);
    }

    /// <summary>
    /// Gets a tenant-supplier relationship by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [HasPermission("supplier.view")]
    [ProducesResponseType(typeof(TenantSupplierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid tenantId,
        Guid id,
        [FromQuery] QueryParameters qp,
        CancellationToken ct)
    {
        var result = await _supplierService.GetTenantSupplierByIdAsync(tenantId, id, qp, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>
    /// Updates a tenant-supplier relationship (partial update).
    /// </summary>
    [HttpPatch("{id:guid}")]
    [HasPermission("supplier.manage")]
    [ProducesResponseType(typeof(TenantSupplierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(
        Guid tenantId,
        Guid id,
        [FromBody] UpdateTenantSupplierRequest request,
        CancellationToken ct)
    {
        var result = await _supplierService.UpdateTenantSupplierAsync(tenantId, id, request, ct);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error }),
                "VALIDATION_ERROR" => BadRequest(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error })
            };
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Unlinks a supplier from this tenant (deletes the relationship).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission("supplier.manage")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unlink(
        Guid tenantId,
        Guid id,
        CancellationToken ct)
    {
        var result = await _supplierService.UnlinkSupplierFromTenantAsync(tenantId, id, ct);

        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });

        return NoContent();
    }
}

#endregion
