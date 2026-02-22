namespace Supplier.Contracts.DTOs;

/// <summary>
/// Supplier data transfer object.
/// </summary>
public sealed record SupplierDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string? NameAr { get; init; }
    public string Country { get; init; } = string.Empty;
    public string? City { get; init; }
    public string? LicenseNumber { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? Website { get; init; }
    public string? Notes { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
    public List<SupplierContactDto>? Contacts { get; init; }
}
