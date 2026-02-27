using System.ComponentModel.DataAnnotations;

namespace Contract.Contracts.DTOs;

public sealed record CreateContractRequest
{
    [Required]
    public Guid WorkerId { get; init; }

    [Required]
    public Guid ClientId { get; init; }

    [Required]
    [MaxLength(30)]
    public string Type { get; init; } = string.Empty;

    [Required]
    public DateOnly StartDate { get; init; }

    public DateOnly? EndDate { get; init; }

    public DateOnly? ProbationEndDate { get; init; }

    public DateOnly? GuaranteeEndDate { get; init; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Rate { get; init; }

    [Required]
    [MaxLength(20)]
    public string RatePeriod { get; init; } = "Monthly";

    [MaxLength(10)]
    public string Currency { get; init; } = "AED";

    public decimal? TotalValue { get; init; }

    public Guid? OriginalContractId { get; init; }

    [MaxLength(2000)]
    public string? Notes { get; init; }
}
