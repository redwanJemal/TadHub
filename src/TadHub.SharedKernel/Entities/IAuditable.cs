namespace TadHub.SharedKernel.Entities;

/// <summary>
/// Interface for entities that track who created and updated them.
/// Implement this interface to automatically populate CreatedBy/UpdatedBy via EF interceptors.
/// </summary>
public interface IAuditable
{
    /// <summary>
    /// The user who created this entity.
    /// </summary>
    Guid? CreatedBy { get; set; }

    /// <summary>
    /// The user who last updated this entity.
    /// </summary>
    Guid? UpdatedBy { get; set; }
}
