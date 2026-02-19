namespace SaasKit.SharedKernel.Entities;

/// <summary>
/// Base class for entities that support soft deletion.
/// Soft-deleted entities are excluded from queries by default via global query filters.
/// </summary>
public abstract class SoftDeletableEntity : TenantScopedEntity
{
    /// <summary>
    /// Indicates whether this entity has been soft-deleted.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Timestamp when the entity was soft-deleted.
    /// Null if the entity is not deleted.
    /// </summary>
    public DateTimeOffset? DeletedAt { get; set; }

    /// <summary>
    /// The user who deleted this entity.
    /// Null if the entity is not deleted.
    /// </summary>
    public Guid? DeletedBy { get; set; }

    /// <summary>
    /// Marks the entity as deleted.
    /// </summary>
    public void MarkAsDeleted(Guid? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = deletedBy;
    }

    /// <summary>
    /// Restores a soft-deleted entity.
    /// </summary>
    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }
}
