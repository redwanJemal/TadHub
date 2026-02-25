using System.ComponentModel.DataAnnotations;

namespace Candidate.Contracts.DTOs;

/// <summary>
/// Request to transition a candidate's status.
/// </summary>
public sealed record TransitionStatusRequest
{
    /// <summary>
    /// Target status to transition to. Required.
    /// </summary>
    [Required]
    [MaxLength(30)]
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Reason for the transition. Required for terminal/failure statuses.
    /// </summary>
    [MaxLength(500)]
    public string? Reason { get; init; }

    /// <summary>
    /// Optional notes about the transition.
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; init; }
}
