namespace Contract.Contracts.DTOs;

public sealed record ContractStatusHistoryDto
{
    public Guid Id { get; init; }
    public Guid ContractId { get; init; }
    public string? FromStatus { get; init; }
    public string ToStatus { get; init; } = string.Empty;
    public DateTimeOffset ChangedAt { get; init; }
    public Guid? ChangedBy { get; init; }
    public string? Reason { get; init; }
    public string? Notes { get; init; }
}
