using ApiManagement.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace ApiManagement.Contracts;

public interface IApiKeyService
{
    Task<PagedList<ApiKeyDto>> GetApiKeysAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);
    Task<Result<ApiKeyDto>> GetApiKeyByIdAsync(Guid tenantId, Guid apiKeyId, CancellationToken ct = default);
    Task<Result<ApiKeyWithSecretDto>> CreateApiKeyAsync(Guid tenantId, CreateApiKeyRequest request, CancellationToken ct = default);
    Task<Result<bool>> RevokeApiKeyAsync(Guid tenantId, Guid apiKeyId, CancellationToken ct = default);
    Task<PagedList<ApiKeyLogDto>> GetApiKeyLogsAsync(Guid tenantId, Guid apiKeyId, QueryParameters qp, CancellationToken ct = default);
    Task<Result<ApiKeyDto>> ValidateApiKeyAsync(string apiKey, CancellationToken ct = default);
    Task RecordApiKeyUsageAsync(Guid apiKeyId, string endpoint, string method, int statusCode, int durationMs, string? ipAddress, CancellationToken ct = default);
}
