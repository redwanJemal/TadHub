namespace Tenancy.Core.Entities;

/// <summary>
/// Types of licenses for Tadbeer agencies.
/// </summary>
public enum LicenseType
{
    Tadbeer = 1,
    MoHRE = 2,
    Trade = 3
}

/// <summary>
/// Status of a license.
/// </summary>
public enum LicenseStatus
{
    Active = 1,
    Expired = 2,
    Suspended = 3
}
