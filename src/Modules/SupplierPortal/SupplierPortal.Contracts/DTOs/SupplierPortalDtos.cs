using System.ComponentModel.DataAnnotations;

namespace SupplierPortal.Contracts.DTOs;

/// <summary>
/// Supplier user account DTO.
/// </summary>
public sealed record SupplierUserDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid SupplierId { get; init; }
    public bool IsActive { get; init; }
    public string? DisplayName { get; init; }
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? SupplierNameEn { get; init; }
    public string? SupplierNameAr { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>
/// Create supplier user request.
/// </summary>
public sealed record CreateSupplierUserRequest
{
    [Required]
    public Guid UserId { get; init; }

    [Required]
    public Guid SupplierId { get; init; }

    [MaxLength(200)]
    public string? DisplayName { get; init; }

    [MaxLength(200)]
    public string? Email { get; init; }

    [MaxLength(50)]
    public string? Phone { get; init; }
}

/// <summary>
/// Update supplier user request (PATCH semantics).
/// </summary>
public sealed record UpdateSupplierUserRequest
{
    public bool? IsActive { get; init; }

    [MaxLength(200)]
    public string? DisplayName { get; init; }

    [MaxLength(200)]
    public string? Email { get; init; }

    [MaxLength(50)]
    public string? Phone { get; init; }
}

/// <summary>
/// Dashboard stats for the supplier portal.
/// </summary>
public sealed record SupplierDashboardDto
{
    public int TotalCandidates { get; init; }
    public int PendingCandidates { get; init; }
    public int ApprovedCandidates { get; init; }
    public int RejectedCandidates { get; init; }
    public int TotalWorkers { get; init; }
    public int ActiveWorkers { get; init; }
    public int DeployedWorkers { get; init; }
    public decimal TotalCommissions { get; init; }
    public decimal PendingCommissions { get; init; }
    public decimal PaidCommissions { get; init; }
}

/// <summary>
/// Lightweight candidate view for supplier portal.
/// </summary>
public sealed record SupplierCandidateListDto
{
    public Guid Id { get; init; }
    public string? FullNameEn { get; init; }
    public string? FullNameAr { get; init; }
    public string? Nationality { get; init; }
    public string? Status { get; init; }
    public string? PhotoUrl { get; init; }
    public string? PassportNumber { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Lightweight worker view for supplier portal.
/// </summary>
public sealed record SupplierWorkerListDto
{
    public Guid Id { get; init; }
    public string? WorkerCode { get; init; }
    public string? FullNameEn { get; init; }
    public string? FullNameAr { get; init; }
    public string? Nationality { get; init; }
    public string? Status { get; init; }
    public string? PhotoUrl { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Supplier commission payment view.
/// </summary>
public sealed record SupplierCommissionDto
{
    public Guid Id { get; init; }
    public string? ReferenceNumber { get; init; }
    public decimal Amount { get; init; }
    public string? Currency { get; init; }
    public string? Status { get; init; }
    public string? WorkerNameEn { get; init; }
    public string? Notes { get; init; }
    public DateTimeOffset? PaymentDate { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Supplier arrival view.
/// </summary>
public sealed record SupplierArrivalListDto
{
    public Guid Id { get; init; }
    public string? WorkerNameEn { get; init; }
    public string? FlightNumber { get; init; }
    public DateTimeOffset? ArrivalDate { get; init; }
    public string? Status { get; init; }
    public string? AirportCode { get; init; }
    public bool HasPreTravelPhoto { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
