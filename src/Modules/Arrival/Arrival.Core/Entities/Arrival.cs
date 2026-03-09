using TadHub.SharedKernel.Entities;

namespace Arrival.Core.Entities;

public class Arrival : SoftDeletableEntity, IAuditable
{
    public string ArrivalCode { get; set; } = string.Empty;
    public ArrivalStatus Status { get; set; } = ArrivalStatus.Scheduled;
    public DateTimeOffset StatusChangedAt { get; set; }

    // Cross-module refs (GUIDs only, no EF FKs)
    public Guid WorkerId { get; set; }
    public Guid PlacementId { get; set; }
    public Guid? SupplierId { get; set; }

    // Flight info
    public string? FlightNumber { get; set; }
    public string? AirportCode { get; set; }
    public string? AirportName { get; set; }
    public DateOnly ScheduledArrivalDate { get; set; }
    public TimeOnly? ScheduledArrivalTime { get; set; }
    public DateTimeOffset? ActualArrivalTime { get; set; }

    // Photos
    public string? PreTravelPhotoUrl { get; set; }
    public string? ArrivalPhotoUrl { get; set; }
    public string? DriverPickupPhotoUrl { get; set; }

    // Driver
    public Guid? DriverId { get; set; }
    public string? DriverName { get; set; }
    public DateTimeOffset? DriverConfirmedPickupAt { get; set; }

    // Accommodation
    public DateTimeOffset? AccommodationConfirmedAt { get; set; }
    public string? AccommodationConfirmedBy { get; set; }

    // Customer pickup
    public bool CustomerPickedUp { get; set; }
    public DateTimeOffset? CustomerPickupConfirmedAt { get; set; }

    // Notes
    public string? Notes { get; set; }

    // Audit
    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }

    // Navigation
    public ICollection<ArrivalStatusHistory> StatusHistory { get; set; } = new List<ArrivalStatusHistory>();
}
