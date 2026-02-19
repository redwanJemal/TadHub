using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace FeatureFlags.Contracts;

public record FeatureFlagDto(Guid Id, string Name, string? Description, bool IsEnabled, int? Percentage, DateTimeOffset CreatedAt);
public record CreateFeatureFlagRequest(string Name, string? Description, bool IsEnabled, int? Percentage);
public record UpdateFeatureFlagRequest(string? Description, bool? IsEnabled, int? Percentage);
public record EvaluationResult(string FlagName, bool IsEnabled, string? Reason);

public interface IFeatureFlagService
{
    Task<PagedList<FeatureFlagDto>> GetFlagsAsync(QueryParameters qp, CancellationToken ct = default);
    Task<Result<FeatureFlagDto>> GetFlagByNameAsync(string name, CancellationToken ct = default);
    Task<Result<FeatureFlagDto>> CreateFlagAsync(CreateFeatureFlagRequest request, CancellationToken ct = default);
    Task<Result<FeatureFlagDto>> UpdateFlagAsync(Guid flagId, UpdateFeatureFlagRequest request, CancellationToken ct = default);
    Task<Result<bool>> DeleteFlagAsync(Guid flagId, CancellationToken ct = default);
    Task<EvaluationResult> EvaluateFlagAsync(string name, Guid tenantId, string? planSlug, CancellationToken ct = default);
}
