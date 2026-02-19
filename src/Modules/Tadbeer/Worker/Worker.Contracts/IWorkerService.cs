using TadHub.SharedKernel.Api;
using TadHub.SharedKernel.Models;
using Worker.Contracts.DTOs;

namespace Worker.Contracts;

/// <summary>
/// Service interface for worker management.
/// </summary>
public interface IWorkerService
{
    #region Worker CRUD

    /// <summary>
    /// Creates a new worker with initial CV data.
    /// Assigns status: NewArrival.
    /// Publishes WorkerCreatedEvent.
    /// </summary>
    Task<Result<WorkerDto>> CreateAsync(CreateWorkerRequest request, CancellationToken ct = default);

    /// <summary>
    /// Gets a worker by ID.
    /// </summary>
    /// <param name="id">Worker ID</param>
    /// <param name="includes">Relations to include: skills, languages, media, jobCategory</param>
    Task<Result<WorkerDto>> GetByIdAsync(Guid id, IncludeSet includes, CancellationToken ct = default);

    /// <summary>
    /// Updates a worker's CV details.
    /// Cannot change status via this method (use TransitionStateAsync).
    /// </summary>
    Task<Result<WorkerDto>> UpdateAsync(Guid id, UpdateWorkerRequest request, CancellationToken ct = default);

    /// <summary>
    /// Lists workers with filtering, sorting, and pagination.
    /// 
    /// Supported filters:
    /// - filter[status]=readyForMarket&amp;filter[status]=inTraining (array)
    /// - filter[nationality]=philippines&amp;filter[nationality]=india (array)
    /// - filter[jobCategoryId]=...
    /// - filter[passportLocation]=withAgency
    /// - filter[isAvailableForFlexible]=true
    /// - filter[createdAt][gte]=2026-01-01
    /// 
    /// Includes shared pool workers if agreements are active.
    /// </summary>
    Task<PagedList<WorkerDto>> ListAsync(QueryParameters query, CancellationToken ct = default);

    #endregion

    #region State Machine

    /// <summary>
    /// Transitions a worker to a new state.
    /// Validates the transition, checks preconditions, appends state history.
    /// Publishes WorkerStatusChangedEvent (and special events like WorkerAbscondedEvent).
    /// </summary>
    /// <param name="id">Worker ID</param>
    /// <param name="request">Target state and reason</param>
    /// <returns>Updated worker or failure with specific reason</returns>
    Task<Result<WorkerDto>> TransitionStateAsync(Guid id, WorkerStateTransitionRequest request, CancellationToken ct = default);

    /// <summary>
    /// Gets the state history for a worker.
    /// </summary>
    Task<PagedList<WorkerStateHistoryDto>> GetStateHistoryAsync(Guid id, QueryParameters query, CancellationToken ct = default);

    /// <summary>
    /// Gets valid target states from current state.
    /// Useful for UI to show available actions.
    /// </summary>
    Task<Result<List<string>>> GetValidTransitionsAsync(Guid id, CancellationToken ct = default);

    #endregion

    #region Passport Custody

    /// <summary>
    /// Gets the current passport custody information.
    /// </summary>
    Task<Result<PassportCustodyDto>> GetPassportCustodyAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets the full passport custody history (audit trail).
    /// </summary>
    Task<PagedList<PassportCustodyDto>> GetPassportCustodyHistoryAsync(Guid id, QueryParameters query, CancellationToken ct = default);

    /// <summary>
    /// Transfers passport custody.
    /// Appends to custody history (audit trail).
    /// </summary>
    Task<Result<PassportCustodyDto>> TransferPassportAsync(Guid id, TransferPassportRequest request, CancellationToken ct = default);

    #endregion

    #region Skills & Languages

    /// <summary>
    /// Adds or updates a skill for a worker.
    /// </summary>
    Task<Result<WorkerSkillDto>> UpsertSkillAsync(Guid workerId, CreateWorkerSkillRequest request, CancellationToken ct = default);

    /// <summary>
    /// Removes a skill from a worker.
    /// </summary>
    Task<Result> RemoveSkillAsync(Guid workerId, string skillName, CancellationToken ct = default);

    /// <summary>
    /// Adds or updates a language for a worker.
    /// </summary>
    Task<Result<WorkerLanguageDto>> UpsertLanguageAsync(Guid workerId, CreateWorkerLanguageRequest request, CancellationToken ct = default);

    /// <summary>
    /// Removes a language from a worker.
    /// </summary>
    Task<Result> RemoveLanguageAsync(Guid workerId, string language, CancellationToken ct = default);

    #endregion

    #region Media

    /// <summary>
    /// Adds media to a worker.
    /// </summary>
    Task<Result<WorkerMediaDto>> AddMediaAsync(Guid workerId, AddWorkerMediaRequest request, CancellationToken ct = default);

    /// <summary>
    /// Removes media from a worker.
    /// </summary>
    Task<Result> RemoveMediaAsync(Guid workerId, Guid mediaId, CancellationToken ct = default);

    /// <summary>
    /// Sets a media item as primary.
    /// </summary>
    Task<Result<WorkerMediaDto>> SetPrimaryMediaAsync(Guid workerId, Guid mediaId, CancellationToken ct = default);

    #endregion
}

/// <summary>
/// Service interface for worker search and matchmaking.
/// </summary>
public interface IWorkerSearchService
{
    /// <summary>
    /// Searches workers with complex criteria and returns ranked results.
    /// 
    /// Scoring weights:
    /// - Nationality match: 20%
    /// - Skill ratings: 30%
    /// - Language proficiency: 20%
    /// - Experience: 15%
    /// - Availability: 15%
    /// 
    /// Includes shared pool workers if criteria allows and agreements exist.
    /// </summary>
    Task<PagedList<WorkerSearchResult>> SearchAsync(WorkerSearchCriteria criteria, CancellationToken ct = default);
}

/// <summary>
/// Service interface for nationality-based pricing.
/// </summary>
public interface INationalityPricingService
{
    /// <summary>
    /// Gets the price for a nationality and contract type at a specific point in time.
    /// Handles overlapping date ranges by selecting the most specific match.
    /// </summary>
    Task<Result<NationalityPricingDto>> GetPriceAsync(
        string nationality, 
        string contractType, 
        DateTimeOffset asOf, 
        CancellationToken ct = default);

    /// <summary>
    /// Gets all active pricing for a nationality.
    /// </summary>
    Task<Result<List<NationalityPricingDto>>> GetPricingForNationalityAsync(
        string nationality, 
        CancellationToken ct = default);

    /// <summary>
    /// Lists all pricing rules.
    /// 
    /// Filters:
    /// - filter[nationality]=philippines
    /// - filter[contractType]=traditional
    /// - filter[effectiveFrom][gte]=2026-01-01
    /// </summary>
    Task<PagedList<NationalityPricingDto>> ListAsync(QueryParameters query, CancellationToken ct = default);

    /// <summary>
    /// Creates a new pricing rule.
    /// </summary>
    Task<Result<NationalityPricingDto>> CreateAsync(CreateNationalityPricingRequest request, CancellationToken ct = default);

    /// <summary>
    /// Deletes a pricing rule.
    /// </summary>
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}

/// <summary>
/// Service interface for job categories.
/// </summary>
public interface IJobCategoryService
{
    /// <summary>
    /// Gets all job categories.
    /// </summary>
    Task<List<JobCategoryDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Gets a job category by ID.
    /// </summary>
    Task<Result<JobCategoryDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Gets a job category by MoHRE code.
    /// </summary>
    Task<Result<JobCategoryDto>> GetByCodeAsync(string moHRECode, CancellationToken ct = default);
}
