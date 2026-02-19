using SaasKit.SharedKernel.Entities;

namespace _Template.Core.Entities;

/// <summary>
/// Example entity for the template module.
/// Copy this module and customize for your domain.
/// </summary>
public class TemplateEntity : TenantScopedEntity
{
    /// <summary>
    /// Name of the entity. Required, max 256 chars.
    /// Filterable: filter[name]=value, filter[name][contains]=partial
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description. Optional, max 1024 chars.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this entity is active.
    /// Filterable: filter[isActive]=true
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Display order for sorting.
    /// Sortable: sort=displayOrder, sort=-displayOrder
    /// </summary>
    public int DisplayOrder { get; set; }
}
