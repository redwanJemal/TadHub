using TadHub.SharedKernel.Entities;

namespace ClientManagement.Core.Entities;

/// <summary>
/// Sales lead entity.
/// Tracks potential clients through the sales funnel.
/// </summary>
public class Lead : TenantScopedEntity, IAuditable
{
    /// <summary>
    /// Client ID if lead was converted.
    /// Null until converted.
    /// </summary>
    public Guid? ClientId { get; set; }

    /// <summary>
    /// Navigation property for converted client.
    /// </summary>
    public Client? Client { get; set; }

    /// <summary>
    /// How the lead was acquired.
    /// </summary>
    public LeadSource Source { get; set; }

    /// <summary>
    /// Current status in the sales funnel.
    /// </summary>
    public LeadStatus Status { get; set; } = LeadStatus.New;

    /// <summary>
    /// Contact name (before conversion).
    /// </summary>
    public string? ContactName { get; set; }

    /// <summary>
    /// Contact phone.
    /// </summary>
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Contact email.
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Notes about the lead.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// User assigned to follow up on this lead.
    /// </summary>
    public Guid? AssignedToUserId { get; set; }

    /// <summary>
    /// When the lead was converted to a client.
    /// </summary>
    public DateTimeOffset? ConvertedAt { get; set; }

    #region IAuditable

    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    #endregion
}
