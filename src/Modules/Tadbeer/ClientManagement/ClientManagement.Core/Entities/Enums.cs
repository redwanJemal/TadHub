namespace ClientManagement.Core.Entities;

/// <summary>
/// Client category based on residency status.
/// </summary>
public enum ClientCategory
{
    /// <summary>UAE National</summary>
    Local = 1,
    /// <summary>Expatriate resident</summary>
    Expat = 2,
    /// <summary>Investor visa holder</summary>
    Investor = 3,
    /// <summary>VIP/High-value client</summary>
    VIP = 4
}

/// <summary>
/// Client's sponsor file status with immigration.
/// </summary>
public enum SponsorFileStatus
{
    /// <summary>File opened, pending documentation</summary>
    Open = 1,
    /// <summary>Documentation submitted, awaiting approval</summary>
    Pending = 2,
    /// <summary>File active, can sponsor workers</summary>
    Active = 3,
    /// <summary>File blocked due to violations</summary>
    Blocked = 4
}

/// <summary>
/// Types of client documents.
/// </summary>
public enum ClientDocumentType
{
    EmiratesId = 1,
    Passport = 2,
    SalaryCertificate = 3,
    EjariContract = 4,
    TenancyContract = 5,
    Other = 99
}

/// <summary>
/// Communication channels.
/// </summary>
public enum CommunicationChannel
{
    Phone = 1,
    WhatsApp = 2,
    Email = 3,
    WalkIn = 4
}

/// <summary>
/// Communication direction.
/// </summary>
public enum CommunicationDirection
{
    Inbound = 1,
    Outbound = 2
}

/// <summary>
/// Lead source channels.
/// </summary>
public enum LeadSource
{
    WalkIn = 1,
    Phone = 2,
    Online = 3,
    Referral = 4,
    SocialMedia = 5
}

/// <summary>
/// Lead status in the sales funnel.
/// </summary>
public enum LeadStatus
{
    New = 1,
    Contacted = 2,
    Qualified = 3,
    Converted = 4,
    Lost = 5
}

/// <summary>
/// Discount card types.
/// </summary>
public enum DiscountCardType
{
    /// <summary>Saada card for people of determination</summary>
    Saada = 1,
    /// <summary>Fazaa card for government employees</summary>
    Fazaa = 2,
    /// <summary>Custom/agency-specific discount</summary>
    Custom = 99
}
