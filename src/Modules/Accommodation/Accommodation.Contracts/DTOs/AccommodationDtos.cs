using System.ComponentModel.DataAnnotations;

namespace Accommodation.Contracts.DTOs;

public sealed record AccommodationStayDto
{
    public Guid Id { get; init; }
    public Guid TenantId { get; init; }
    public string StayCode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset StatusChangedAt { get; init; }
    public Guid WorkerId { get; init; }
    public Guid? PlacementId { get; init; }
    public Guid? ArrivalId { get; init; }
    public DateTimeOffset CheckInDate { get; init; }
    public DateTimeOffset? CheckOutDate { get; init; }
    public string? Room { get; init; }
    public string? Location { get; init; }
    public string? DepartureReason { get; init; }
    public string? DepartureNotes { get; init; }
    public string CheckedInBy { get; init; } = string.Empty;
    public string? CheckedOutBy { get; init; }

    // BFF-enriched
    public AccommodationWorkerRefDto? Worker { get; init; }

    // Audit
    public Guid? CreatedBy { get; init; }
    public Guid? UpdatedBy { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public sealed record AccommodationStayListDto
{
    public Guid Id { get; init; }
    public string StayCode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public Guid WorkerId { get; init; }
    public DateTimeOffset CheckInDate { get; init; }
    public DateTimeOffset? CheckOutDate { get; init; }
    public string? Room { get; init; }
    public string? Location { get; init; }
    public string? DepartureReason { get; init; }
    public string CheckedInBy { get; init; } = string.Empty;

    // BFF-enriched
    public AccommodationWorkerRefDto? Worker { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}

public sealed record AccommodationWorkerRefDto
{
    public Guid Id { get; init; }
    public string FullNameEn { get; init; } = string.Empty;
    public string? FullNameAr { get; init; }
    public string? WorkerCode { get; init; }
    public string? PhotoUrl { get; init; }
}

public sealed record CheckInRequest
{
    [Required]
    public Guid WorkerId { get; init; }

    public Guid? PlacementId { get; init; }
    public Guid? ArrivalId { get; init; }

    [MaxLength(50)]
    public string? Room { get; init; }

    [MaxLength(200)]
    public string? Location { get; init; }
}

public sealed record CheckOutRequest
{
    [Required]
    [MaxLength(30)]
    public string DepartureReason { get; init; } = string.Empty;

    [MaxLength(1000)]
    public string? DepartureNotes { get; init; }
}

public sealed record UpdateStayRequest
{
    [MaxLength(50)]
    public string? Room { get; init; }

    [MaxLength(200)]
    public string? Location { get; init; }
}
