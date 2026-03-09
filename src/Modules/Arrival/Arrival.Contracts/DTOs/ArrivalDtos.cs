using System.ComponentModel.DataAnnotations;
using TadHub.SharedKernel.Api;

namespace Arrival.Contracts.DTOs;

public sealed record ArrivalDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string ArrivalCode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset StatusChangedAt { get; init; }

    // Cross-module refs
    public Guid WorkerId { get; init; }
    public Guid PlacementId { get; init; }
    public Guid? SupplierId { get; init; }

    // Enriched refs (BFF)
    public ArrivalWorkerRefDto? Worker { get; init; }

    // Flight info
    public string? FlightNumber { get; init; }
    public string? AirportCode { get; init; }
    public string? AirportName { get; init; }
    public DateOnly ScheduledArrivalDate { get; init; }
    public TimeOnly? ScheduledArrivalTime { get; init; }
    public DateTimeOffset? ActualArrivalTime { get; init; }

    // Photos
    public string? PreTravelPhotoUrl { get; init; }
    public string? ArrivalPhotoUrl { get; init; }
    public string? DriverPickupPhotoUrl { get; init; }

    // Driver
    public Guid? DriverId { get; init; }
    public string? DriverName { get; init; }
    public DateTimeOffset? DriverConfirmedPickupAt { get; init; }

    // Accommodation
    public DateTimeOffset? AccommodationConfirmedAt { get; init; }
    public string? AccommodationConfirmedBy { get; init; }

    // Customer pickup
    public bool CustomerPickedUp { get; init; }
    public DateTimeOffset? CustomerPickupConfirmedAt { get; init; }

    // Notes
    public string? Notes { get; init; }

    // Audit
    public Guid? CreatedBy { get; init; }
    public Guid? UpdatedBy { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    // Includes
    public List<ArrivalStatusHistoryDto>? StatusHistory { get; init; }
}

public sealed record ArrivalListDto
{
    public Guid Id { get; init; }
    public string ArrivalCode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset StatusChangedAt { get; init; }
    public Guid WorkerId { get; init; }
    public Guid PlacementId { get; init; }
    public Guid? SupplierId { get; init; }
    public ArrivalWorkerRefDto? Worker { get; init; }
    public string? FlightNumber { get; init; }
    public string? AirportCode { get; init; }
    public DateOnly ScheduledArrivalDate { get; init; }
    public TimeOnly? ScheduledArrivalTime { get; init; }
    public DateTimeOffset? ActualArrivalTime { get; init; }
    public Guid? DriverId { get; init; }
    public string? DriverName { get; init; }
    public bool CustomerPickedUp { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record ArrivalWorkerRefDto
{
    public Guid Id { get; init; }
    public string FullNameEn { get; init; } = string.Empty;
    public string? FullNameAr { get; init; }
    public string? WorkerCode { get; init; }
    public string? PhotoUrl { get; init; }
}

public sealed record ArrivalStatusHistoryDto
{
    public Guid Id { get; init; }
    public Guid ArrivalId { get; init; }
    public string? FromStatus { get; init; }
    public string ToStatus { get; init; } = string.Empty;
    public DateTimeOffset ChangedAt { get; init; }
    public string? ChangedBy { get; init; }
    public string? Reason { get; init; }
    public string? Notes { get; init; }
}

public sealed record ScheduleArrivalRequest
{
    [Required]
    public Guid PlacementId { get; init; }

    [Required]
    public Guid WorkerId { get; init; }

    public Guid? SupplierId { get; init; }

    [MaxLength(50)]
    public string? FlightNumber { get; init; }

    [MaxLength(10)]
    public string? AirportCode { get; init; }

    [MaxLength(200)]
    public string? AirportName { get; init; }

    [Required]
    public DateOnly ScheduledArrivalDate { get; init; }

    public TimeOnly? ScheduledArrivalTime { get; init; }

    [MaxLength(2000)]
    public string? Notes { get; init; }
}

public sealed record UpdateArrivalRequest
{
    [MaxLength(50)]
    public string? FlightNumber { get; init; }

    [MaxLength(10)]
    public string? AirportCode { get; init; }

    [MaxLength(200)]
    public string? AirportName { get; init; }

    public DateOnly? ScheduledArrivalDate { get; init; }

    public TimeOnly? ScheduledArrivalTime { get; init; }

    [MaxLength(2000)]
    public string? Notes { get; init; }
}

public sealed record AssignDriverRequest
{
    [Required]
    public Guid DriverId { get; init; }

    [Required]
    [MaxLength(200)]
    public string DriverName { get; init; } = string.Empty;
}

public sealed record ConfirmArrivalRequest
{
    public DateTimeOffset? ActualArrivalTime { get; init; }

    [MaxLength(2000)]
    public string? Notes { get; init; }
}

public sealed record ConfirmPickupRequest
{
    [MaxLength(2000)]
    public string? Notes { get; init; }
}

public sealed record ConfirmAccommodationRequest
{
    [MaxLength(200)]
    public string? ConfirmedBy { get; init; }

    [MaxLength(2000)]
    public string? Notes { get; init; }
}

public sealed record ConfirmCustomerPickupRequest
{
    [MaxLength(2000)]
    public string? Notes { get; init; }
}

public sealed record ReportNoShowRequest
{
    [MaxLength(500)]
    public string? Reason { get; init; }

    [MaxLength(2000)]
    public string? Notes { get; init; }
}
