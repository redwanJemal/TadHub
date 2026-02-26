namespace Candidate.Contracts.DTOs;

public sealed record CandidateSupplierDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
