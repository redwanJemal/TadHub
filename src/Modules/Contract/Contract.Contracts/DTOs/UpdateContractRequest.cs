using System.ComponentModel.DataAnnotations;

namespace Contract.Contracts.DTOs;

public sealed record UpdateContractRequest
{
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public DateOnly? ProbationEndDate { get; init; }
    public DateOnly? GuaranteeEndDate { get; init; }
    public bool? ProbationPassed { get; init; }

    [Range(0, double.MaxValue)]
    public decimal? Rate { get; init; }

    [MaxLength(20)]
    public string? RatePeriod { get; init; }

    [MaxLength(10)]
    public string? Currency { get; init; }

    public decimal? TotalValue { get; init; }

    [MaxLength(2000)]
    public string? Notes { get; init; }
}
