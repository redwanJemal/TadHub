using Candidate.Contracts.DTOs;
using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;

namespace Candidate.Contracts;

/// <summary>
/// Service for managing candidates within a tenant.
/// </summary>
public interface ICandidateService
{
    /// <summary>
    /// Lists candidates for a tenant with filtering, sorting, search, and pagination.
    /// </summary>
    Task<PagedList<CandidateListDto>> ListAsync(Guid tenantId, QueryParameters qp, CancellationToken ct = default);

    /// <summary>
    /// Gets a candidate by ID.
    /// </summary>
    Task<Result<CandidateDto>> GetByIdAsync(Guid tenantId, Guid id, QueryParameters? qp = null, CancellationToken ct = default);

    /// <summary>
    /// Creates a new candidate.
    /// </summary>
    Task<Result<CandidateDto>> CreateAsync(Guid tenantId, CreateCandidateRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates a candidate (partial update).
    /// </summary>
    Task<Result<CandidateDto>> UpdateAsync(Guid tenantId, Guid id, UpdateCandidateRequest request, CancellationToken ct = default);

    /// <summary>
    /// Transitions a candidate's status.
    /// </summary>
    Task<Result<CandidateDto>> TransitionStatusAsync(Guid tenantId, Guid id, TransitionStatusRequest request, CancellationToken ct = default);

    /// <summary>
    /// Gets the status history for a candidate.
    /// </summary>
    Task<Result<List<CandidateStatusHistoryDto>>> GetStatusHistoryAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    /// <summary>
    /// Soft deletes a candidate.
    /// </summary>
    Task<Result> DeleteAsync(Guid tenantId, Guid id, CancellationToken ct = default);

    /// <summary>
    /// Returns candidate counts grouped by status for the given tenant.
    /// </summary>
    Task<Dictionary<string, int>> GetCountsByStatusAsync(Guid tenantId, CancellationToken ct = default);
}
