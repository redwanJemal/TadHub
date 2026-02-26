using TadHub.SharedKernel.Entities;

namespace Client.Core.Entities;

public class Client : TenantScopedEntity
{
    public string NameEn { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? NationalId { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}
