using System.ComponentModel.DataAnnotations;

namespace ReferenceData.Contracts.DTOs;

/// <summary>
/// Full country package DTO with all cost components.
/// </summary>
public sealed record CountryPackageDto
{
    public Guid Id { get; init; }
    public Guid CountryId { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsDefault { get; init; }

    // Country reference
    public string? CountryNameEn { get; init; }
    public string? CountryNameAr { get; init; }
    public string? CountryCode { get; init; }

    // Cost components
    public decimal MaidCost { get; init; }
    public decimal MonthlyAccommodationCost { get; init; }
    public decimal VisaCost { get; init; }
    public decimal EmploymentVisaCost { get; init; }
    public decimal ResidenceVisaCost { get; init; }
    public decimal MedicalCost { get; init; }
    public decimal TransportationCost { get; init; }
    public decimal TicketCost { get; init; }
    public decimal InsuranceCost { get; init; }
    public decimal EmiratesIdCost { get; init; }
    public decimal OtherCosts { get; init; }
    public decimal TotalPackagePrice { get; init; }

    // Supplier commission
    public decimal SupplierCommission { get; init; }
    public string SupplierCommissionType { get; init; } = string.Empty;

    // Defaults
    public string DefaultGuaranteePeriod { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;

    // Effective dates
    public string EffectiveFrom { get; init; } = string.Empty;
    public string? EffectiveTo { get; init; }
    public bool IsActive { get; init; }
    public string? Notes { get; init; }

    // Audit
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>
/// Lightweight country package for list views.
/// </summary>
public sealed record CountryPackageListDto
{
    public Guid Id { get; init; }
    public Guid CountryId { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsDefault { get; init; }
    public string? CountryNameEn { get; init; }
    public string? CountryNameAr { get; init; }
    public string? CountryCode { get; init; }
    public decimal TotalPackagePrice { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string EffectiveFrom { get; init; } = string.Empty;
    public string? EffectiveTo { get; init; }
    public bool IsActive { get; init; }
    public string DefaultGuaranteePeriod { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Request to create a country package.
/// </summary>
public sealed record CreateCountryPackageRequest
{
    [Required]
    public Guid CountryId { get; init; }

    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = string.Empty;

    public bool IsDefault { get; init; }

    public decimal MaidCost { get; init; }
    public decimal MonthlyAccommodationCost { get; init; }
    public decimal VisaCost { get; init; }
    public decimal EmploymentVisaCost { get; init; }
    public decimal ResidenceVisaCost { get; init; }
    public decimal MedicalCost { get; init; }
    public decimal TransportationCost { get; init; }
    public decimal TicketCost { get; init; }
    public decimal InsuranceCost { get; init; }
    public decimal EmiratesIdCost { get; init; }
    public decimal OtherCosts { get; init; }
    public decimal TotalPackagePrice { get; init; }

    public decimal SupplierCommission { get; init; }
    public string SupplierCommissionType { get; init; } = "FixedAmount";

    public string DefaultGuaranteePeriod { get; init; } = "TwoYears";
    public string Currency { get; init; } = "AED";

    [Required]
    public string EffectiveFrom { get; init; } = string.Empty;
    public string? EffectiveTo { get; init; }

    public bool IsActive { get; init; } = true;

    [MaxLength(2000)]
    public string? Notes { get; init; }
}

/// <summary>
/// Request to update a country package (PATCH semantics — all fields nullable).
/// </summary>
public sealed record UpdateCountryPackageRequest
{
    public Guid? CountryId { get; init; }

    [MaxLength(200)]
    public string? Name { get; init; }

    public bool? IsDefault { get; init; }

    public decimal? MaidCost { get; init; }
    public decimal? MonthlyAccommodationCost { get; init; }
    public decimal? VisaCost { get; init; }
    public decimal? EmploymentVisaCost { get; init; }
    public decimal? ResidenceVisaCost { get; init; }
    public decimal? MedicalCost { get; init; }
    public decimal? TransportationCost { get; init; }
    public decimal? TicketCost { get; init; }
    public decimal? InsuranceCost { get; init; }
    public decimal? EmiratesIdCost { get; init; }
    public decimal? OtherCosts { get; init; }
    public decimal? TotalPackagePrice { get; init; }

    public decimal? SupplierCommission { get; init; }
    public string? SupplierCommissionType { get; init; }

    public string? DefaultGuaranteePeriod { get; init; }
    public string? Currency { get; init; }

    public string? EffectiveFrom { get; init; }
    public string? EffectiveTo { get; init; }

    public bool? IsActive { get; init; }

    [MaxLength(2000)]
    public string? Notes { get; init; }
}
