namespace Worker.Contracts.DTOs;

public sealed record WorkerSkillDto
{
    public Guid Id { get; init; }
    public string SkillName { get; init; } = string.Empty;
    public string ProficiencyLevel { get; init; } = string.Empty;
}
