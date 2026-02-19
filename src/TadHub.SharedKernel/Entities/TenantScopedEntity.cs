namespace TadHub.SharedKernel.Entities;

/// <summary>
/// Base class for entities that belong to a specific tenant.
/// Approximately 90% of entities in the system inherit from this.
/// </summary>
public abstract class TenantScopedEntity : BaseEntity
{
    /// <summary>
    /// The tenant this entity belongs to.
    /// Required for all tenant-scoped entities.
    /// </summary>
    public Guid TenantId { get; set; }
}
