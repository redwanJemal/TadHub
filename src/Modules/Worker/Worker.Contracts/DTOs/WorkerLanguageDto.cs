namespace Worker.Contracts.DTOs;

public sealed record WorkerLanguageDto
{
    public Guid Id { get; init; }
    public string Language { get; init; } = string.Empty;
    public string ProficiencyLevel { get; init; } = string.Empty;
}
