namespace Worker.Contracts.DTOs;

public sealed record WorkerSupplierDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
