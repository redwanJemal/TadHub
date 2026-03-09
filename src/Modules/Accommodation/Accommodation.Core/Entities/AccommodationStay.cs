using TadHub.SharedKernel.Entities;

namespace Accommodation.Core.Entities;

public class AccommodationStay : SoftDeletableEntity, IAuditable
{
    public string StayCode { get; set; } = string.Empty;
    public AccommodationStayStatus Status { get; set; } = AccommodationStayStatus.CheckedIn;
    public DateTimeOffset StatusChangedAt { get; set; }

    // Cross-module refs (GUIDs only, no EF FKs)
    public Guid WorkerId { get; set; }
    public Guid? PlacementId { get; set; }
    public Guid? ArrivalId { get; set; }

    // Stay details
    public DateTimeOffset CheckInDate { get; set; }
    public DateTimeOffset? CheckOutDate { get; set; }
    public string? Room { get; set; }
    public string? Location { get; set; }

    // Departure
    public DepartureReason? DepartureReason { get; set; }
    public string? DepartureNotes { get; set; }

    // Who checked in/out
    public string CheckedInBy { get; set; } = string.Empty;
    public string? CheckedOutBy { get; set; }

    // Audit
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
