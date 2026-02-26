namespace Candidate.Contracts.DTOs;

/// <summary>
/// Candidate language response DTO.
/// </summary>
public sealed record CandidateLanguageDto
{
    public Guid Id { get; init; }
    public string Language { get; init; } = string.Empty;
    public string ProficiencyLevel { get; init; } = string.Empty;
}

/// <summary>
/// Request to add a language to a candidate.
/// </summary>
public sealed record CandidateLanguageRequest
{
    public string Language { get; init; } = string.Empty;
    public string ProficiencyLevel { get; init; } = string.Empty;
}
