using System.ComponentModel.DataAnnotations;

namespace Contract.Contracts.DTOs;

public sealed record TransitionContractStatusRequest
{
    [Required]
    [MaxLength(30)]
    public string Status { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Reason { get; init; }

    [MaxLength(2000)]
    public string? Notes { get; init; }
}
