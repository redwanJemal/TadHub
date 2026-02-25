namespace Candidate.Contracts.DTOs;

/// <summary>
/// Status history entry DTO.
/// </summary>
public sealed record CandidateStatusHistoryDto
{
    public Guid Id { get; init; }
    public Guid CandidateId { get; init; }
    public string? FromStatus { get; init; }
    public string ToStatus { get; init; } = string.Empty;
    public DateTimeOffset ChangedAt { get; init; }
    public Guid? ChangedBy { get; init; }
    public string? Reason { get; init; }
    public string? Notes { get; init; }
}
