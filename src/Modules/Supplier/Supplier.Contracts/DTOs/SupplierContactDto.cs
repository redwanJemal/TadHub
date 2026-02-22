namespace Supplier.Contracts.DTOs;

/// <summary>
/// Supplier contact data transfer object.
/// </summary>
public sealed record SupplierContactDto
{
    public Guid Id { get; init; }
    public Guid SupplierId { get; init; }
    public Guid? UserId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string? JobTitle { get; init; }
    public bool IsPrimary { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}
