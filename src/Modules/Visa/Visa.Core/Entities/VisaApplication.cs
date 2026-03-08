using TadHub.SharedKernel.Entities;

namespace Visa.Core.Entities;

public class VisaApplication : SoftDeletableEntity, IAuditable
{
    public string ApplicationCode { get; set; } = string.Empty;
    public VisaType VisaType { get; set; }
    public VisaApplicationStatus Status { get; set; } = VisaApplicationStatus.NotStarted;
    public DateTimeOffset StatusChangedAt { get; set; }
    public string? StatusReason { get; set; }

    // Cross-module refs (GUIDs only, no EF FKs)
    public Guid WorkerId { get; set; }
    public Guid ClientId { get; set; }
    public Guid? ContractId { get; set; }
    public Guid? PlacementId { get; set; }

    // Dates
    public DateOnly? ApplicationDate { get; set; }
    public DateOnly? ApprovalDate { get; set; }
    public DateOnly? IssuanceDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }

    // Reference info
    public string? ReferenceNumber { get; set; }
    public string? VisaNumber { get; set; }
    public string? Notes { get; set; }
    public string? RejectionReason { get; set; }

    // Audit
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation
    public ICollection<VisaApplicationStatusHistory> StatusHistory { get; set; } = new List<VisaApplicationStatusHistory>();
    public ICollection<VisaApplicationDocument> Documents { get; set; } = new List<VisaApplicationDocument>();
}
