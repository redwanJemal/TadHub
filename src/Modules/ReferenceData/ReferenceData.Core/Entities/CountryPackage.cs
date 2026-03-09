using TadHub.SharedKernel.Entities;

namespace ReferenceData.Core.Entities;

/// <summary>
/// Country payment package — standard pricing schema per country of origin.
/// Tenant-scoped: each Tadbeer center can define its own packages.
/// </summary>
public class CountryPackage : SoftDeletableEntity
{
    /// <summary>
    /// Links to the global Country reference entity.
    /// </summary>
    public Guid CountryId { get; set; }

    /// <summary>
    /// Package name (e.g., "Ethiopia Standard", "Philippines Premium").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is the default package for the country within this tenant.
    /// Only one default per country per tenant.
    /// </summary>
    public bool IsDefault { get; set; }

    // ── Cost components ──

    /// <summary>Base procurement/recruitment cost.</summary>
    public decimal MaidCost { get; set; }

    /// <summary>Monthly accommodation cost.</summary>
    public decimal MonthlyAccommodationCost { get; set; }

    /// <summary>Combined visa cost (employment + residence).</summary>
    public decimal VisaCost { get; set; }

    /// <summary>Employment visa cost (split).</summary>
    public decimal EmploymentVisaCost { get; set; }

    /// <summary>Residence visa cost (split).</summary>
    public decimal ResidenceVisaCost { get; set; }

    /// <summary>Medical examination cost.</summary>
    public decimal MedicalCost { get; set; }

    /// <summary>Local transportation cost.</summary>
    public decimal TransportationCost { get; set; }

    /// <summary>Flight ticket cost.</summary>
    public decimal TicketCost { get; set; }

    /// <summary>Insurance cost.</summary>
    public decimal InsuranceCost { get; set; }

    /// <summary>Emirates ID processing cost.</summary>
    public decimal EmiratesIdCost { get; set; }

    /// <summary>Other miscellaneous costs.</summary>
    public decimal OtherCosts { get; set; }

    /// <summary>Total package price (computed or manual).</summary>
    public decimal TotalPackagePrice { get; set; }

    // ── Supplier commission ──

    /// <summary>Supplier commission amount or percentage value.</summary>
    public decimal SupplierCommission { get; set; }

    /// <summary>Whether commission is a fixed amount or percentage.</summary>
    public SupplierCommissionType SupplierCommissionType { get; set; } = SupplierCommissionType.FixedAmount;

    // ── Defaults ──

    /// <summary>Default guarantee period for contracts using this package.</summary>
    public DefaultGuaranteePeriod DefaultGuaranteePeriod { get; set; } = DefaultGuaranteePeriod.TwoYears;

    /// <summary>Currency code (default AED).</summary>
    public string Currency { get; set; } = "AED";

    // ── Effective dates ──

    /// <summary>Date from which this package is effective.</summary>
    public DateOnly EffectiveFrom { get; set; }

    /// <summary>Date until which this package is effective. Null means currently active.</summary>
    public DateOnly? EffectiveTo { get; set; }

    /// <summary>Whether this package is active for use.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Optional notes.</summary>
    public string? Notes { get; set; }
}

/// <summary>
/// How supplier commission is calculated.
/// </summary>
public enum SupplierCommissionType
{
    FixedAmount = 0,
    Percentage = 1,
}

/// <summary>
/// Default guarantee period options for country packages.
/// </summary>
public enum DefaultGuaranteePeriod
{
    SixMonths = 0,
    OneYear = 1,
    TwoYears = 2,
}
