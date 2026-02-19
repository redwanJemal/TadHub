using SaasKit.SharedKernel.Api;
using SaasKit.SharedKernel.Models;

namespace _Template.Contracts;

/// <summary>
/// DTO for TemplateEntity.
/// </summary>
public record TemplateEntityDto(Guid Id, string Name, string? Description, bool IsActive, int DisplayOrder, DateTimeOffset CreatedAt);

/// <summary>
/// Request to create a new template entity.
/// </summary>
public record CreateTemplateEntityRequest(string Name, string? Description, int DisplayOrder = 0);

/// <summary>
/// Request to update a template entity.
/// </summary>
public record UpdateTemplateEntityRequest(string? Name, string? Description, bool? IsActive, int? DisplayOrder);

/// <summary>
/// Service interface for managing template entities.
/// </summary>
public interface ITemplateService
{
    /// <summary>
    /// Gets entities with filtering and sorting.
    /// Filters: filter[name], filter[isActive]
    /// Sort: sort=name, sort=-createdAt, sort=displayOrder
    /// </summary>
    Task<PagedList<TemplateEntityDto>> GetEntitiesAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);

    /// <summary>
    /// Gets an entity by ID.
    /// </summary>
    Task<Result<TemplateEntityDto>> GetByIdAsync(Guid tenantId, Guid entityId, CancellationToken ct = default);

    /// <summary>
    /// Creates a new entity.
    /// </summary>
    Task<Result<TemplateEntityDto>> CreateAsync(Guid tenantId, CreateTemplateEntityRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates an entity.
    /// </summary>
    Task<Result<TemplateEntityDto>> UpdateAsync(Guid tenantId, Guid entityId, UpdateTemplateEntityRequest request, CancellationToken ct = default);

    /// <summary>
    /// Deletes an entity.
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid tenantId, Guid entityId, CancellationToken ct = default);
}
