using System.ComponentModel.DataAnnotations;

namespace Worker.Contracts.DTOs;

/// <summary>
/// Request to transition a worker's status.
/// </summary>
public sealed record TransitionWorkerStatusRequest
{
    [Required]
    [MaxLength(30)]
    public string Status { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Reason { get; init; }

    [MaxLength(2000)]
    public string? Notes { get; init; }
}
