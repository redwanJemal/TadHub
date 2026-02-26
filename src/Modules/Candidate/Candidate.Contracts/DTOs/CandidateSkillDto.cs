namespace Candidate.Contracts.DTOs;

/// <summary>
/// Candidate skill response DTO.
/// </summary>
public sealed record CandidateSkillDto
{
    public Guid Id { get; init; }
    public string SkillName { get; init; } = string.Empty;
    public string ProficiencyLevel { get; init; } = string.Empty;
}

/// <summary>
/// Request to add a skill to a candidate.
/// </summary>
public sealed record CandidateSkillRequest
{
    public string SkillName { get; init; } = string.Empty;
    public string ProficiencyLevel { get; init; } = string.Empty;
}
