namespace Tenancy.Core.Entities;

/// <summary>
/// Status of a shared pool agreement between agencies.
/// </summary>
public enum SharedPoolStatus
{
    Pending = 1,
    Active = 2,
    Revoked = 3
}
