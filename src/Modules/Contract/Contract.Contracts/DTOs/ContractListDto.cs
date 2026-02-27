namespace Contract.Contracts.DTOs;

public sealed record ContractListDto
{
    public Guid Id { get; init; }
    public string ContractCode { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public Guid WorkerId { get; init; }
    public Guid ClientId { get; init; }
    public ContractWorkerDto? Worker { get; init; }
    public ContractClientDto? Client { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public decimal Rate { get; init; }
    public string RatePeriod { get; init; } = string.Empty;
    public string Currency { get; init; } = "AED";
    public DateTimeOffset CreatedAt { get; init; }
}
