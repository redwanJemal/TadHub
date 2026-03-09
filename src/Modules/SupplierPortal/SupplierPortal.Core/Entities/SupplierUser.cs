using TadHub.SharedKernel.Entities;

namespace SupplierPortal.Core.Entities;

/// <summary>
/// Links a platform user account to a supplier entity, enabling supplier portal access.
/// A supplier user can see only data related to their supplier across linked tenants.
/// </summary>
public class SupplierUser : BaseEntity
{
    /// <summary>
    /// The user's identity ID (from Keycloak / user_profiles).
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The global supplier this user is associated with.
    /// </summary>
    public Guid SupplierId { get; set; }

    /// <summary>
    /// Whether this supplier user account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Optional display name for the supplier user.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Email address for the supplier user.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Phone number for the supplier user.
    /// </summary>
    public string? Phone { get; set; }
}
