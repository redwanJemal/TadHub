using TadHub.SharedKernel.Entities;
using TadHub.SharedKernel.Enums;

namespace ClientManagement.Core.Entities;

/// <summary>
/// Client (employer) entity.
/// Represents a potential or actual employer of domestic workers.
/// </summary>
public class Client : TenantScopedEntity, IAuditable
{
    /// <summary>
    /// UAE Emirates ID number (unique within tenant).
    /// </summary>
    public string EmiratesId { get; set; } = string.Empty;

    /// <summary>
    /// Full name in English.
    /// </summary>
    public string FullNameEn { get; set; } = string.Empty;

    /// <summary>
    /// Full name in Arabic.
    /// </summary>
    public string FullNameAr { get; set; } = string.Empty;

    /// <summary>
    /// Passport number.
    /// </summary>
    public string? PassportNumber { get; set; }

    /// <summary>
    /// Nationality (e.g., "UAE", "India", "Philippines").
    /// </summary>
    public string Nationality { get; set; } = string.Empty;

    /// <summary>
    /// Contact phone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Contact email.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Client category based on residency/status.
    /// </summary>
    public ClientCategory Category { get; set; }

    /// <summary>
    /// Sponsor file status with immigration.
    /// </summary>
    public SponsorFileStatus SponsorFileStatus { get; set; } = SponsorFileStatus.Open;

    /// <summary>
    /// UAE Emirate of residence.
    /// </summary>
    public Emirate? Emirate { get; set; }

    /// <summary>
    /// Whether client documents have been verified.
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// When the client was verified.
    /// </summary>
    public DateTimeOffset? VerifiedAt { get; set; }

    /// <summary>
    /// User who verified the client.
    /// </summary>
    public Guid? VerifiedByUserId { get; set; }

    /// <summary>
    /// Reason if client is blocked.
    /// </summary>
    public string? BlockedReason { get; set; }

    /// <summary>
    /// Additional notes about the client.
    /// </summary>
    public string? Notes { get; set; }

    #region IAuditable

    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Client documents.
    /// </summary>
    public ICollection<ClientDocument> Documents { get; set; } = new List<ClientDocument>();

    /// <summary>
    /// Communication logs.
    /// </summary>
    public ICollection<ClientCommunicationLog> CommunicationLogs { get; set; } = new List<ClientCommunicationLog>();

    /// <summary>
    /// Discount cards.
    /// </summary>
    public ICollection<DiscountCard> DiscountCards { get; set; } = new List<DiscountCard>();

    /// <summary>
    /// Leads that converted to this client.
    /// </summary>
    public ICollection<Lead> Leads { get; set; } = new List<Lead>();

    #endregion

    /// <summary>
    /// Display name (English name).
    /// </summary>
    public string DisplayName => FullNameEn;
}
