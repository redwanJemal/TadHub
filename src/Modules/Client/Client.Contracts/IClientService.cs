using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Client.Contracts;

public record ClientDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string? NameAr { get; init; }
    public string? NationalId { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? Notes { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

public record ClientListDto
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = string.Empty;
    public string? NameAr { get; init; }
    public string? NationalId { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? City { get; init; }
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

public record CreateClientRequest
{
    public string NameEn { get; init; } = string.Empty;
    public string? NameAr { get; init; }
    public string? NationalId { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? Notes { get; init; }
}

public record UpdateClientRequest
{
    public string? NameEn { get; init; }
    public string? NameAr { get; init; }
    public string? NationalId { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? Notes { get; init; }
    public bool? IsActive { get; init; }
}

public interface IClientService
{
    Task<PagedList<ClientListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);
    Task<Result<ClientDto>> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<Result<ClientDto>> CreateAsync(Guid tenantId, CreateClientRequest request, CancellationToken ct = default);
    Task<Result<ClientDto>> UpdateAsync(Guid tenantId, Guid id, UpdateClientRequest request, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default);
}
