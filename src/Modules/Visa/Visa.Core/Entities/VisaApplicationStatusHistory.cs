using TadHub.SharedKernel.Entities;

namespace Visa.Core.Entities;

public class VisaApplicationStatusHistory : TenantScopedEntity
{
    public Guid VisaApplicationId { get; set; }
    public VisaApplicationStatus? FromStatus { get; set; }
    public VisaApplicationStatus ToStatus { get; set; }
    public DateTimeOffset ChangedAt { get; set; }
    public string? ChangedBy { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public VisaApplication VisaApplication { get; set; } = null!;
}
