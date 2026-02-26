namespace Worker.Contracts.DTOs;

public sealed record WorkerStatusHistoryDto
{
    public Guid Id { get; init; }
    public Guid WorkerId { get; init; }
    public string? FromStatus { get; init; }
    public string ToStatus { get; init; } = string.Empty;
    public DateTimeOffset ChangedAt { get; init; }
    public Guid? ChangedBy { get; init; }
    public string? Reason { get; init; }
    public string? Notes { get; init; }
}
