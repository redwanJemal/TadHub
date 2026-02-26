using Candidate.Core.Entities;

namespace Candidate.Core.Services;

/// <summary>
/// Validates candidate status transitions.
/// </summary>
public static class CandidateStatusMachine
{
    /// <summary>
    /// Valid transitions (same for all source types).
    /// Approved, Rejected, Cancelled are terminal — no further transitions.
    /// </summary>
    private static readonly Dictionary<CandidateStatus, HashSet<CandidateStatus>> Transitions = new()
    {
        [CandidateStatus.Received] = [CandidateStatus.UnderReview, CandidateStatus.Cancelled],
        [CandidateStatus.UnderReview] = [CandidateStatus.Approved, CandidateStatus.Rejected, CandidateStatus.Cancelled],
    };

    /// <summary>
    /// Statuses that require a reason when transitioning to them.
    /// </summary>
    private static readonly HashSet<CandidateStatus> ReasonRequired =
    [
        CandidateStatus.Rejected,
        CandidateStatus.Cancelled,
    ];

    /// <summary>
    /// Validates a status transition.
    /// </summary>
    /// <param name="sourceType">The candidate's source type (kept for backward compat, ignored internally).</param>
    /// <param name="from">Current status.</param>
    /// <param name="to">Target status.</param>
    /// <param name="reason">Reason for the transition.</param>
    /// <returns>Null if valid; error message string if invalid.</returns>
    public static string? Validate(CandidateSourceType sourceType, CandidateStatus from, CandidateStatus to, string? reason)
    {
        if (!Transitions.TryGetValue(from, out var validTargets))
            return $"Status '{from}' is a terminal status and cannot be transitioned";

        if (!validTargets.Contains(to))
            return $"Transition from '{from}' to '{to}' is not allowed";

        if (ReasonRequired.Contains(to) && string.IsNullOrWhiteSpace(reason))
            return $"A reason is required when transitioning to '{to}'";

        return null;
    }
}
