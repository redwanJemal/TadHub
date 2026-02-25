using Candidate.Core.Entities;

namespace Candidate.Core.Services;

/// <summary>
/// Validates candidate status transitions based on source type.
/// </summary>
public static class CandidateStatusMachine
{
    /// <summary>
    /// Valid transitions for supplier-sourced candidates (full procurement pipeline).
    /// </summary>
    private static readonly Dictionary<CandidateStatus, HashSet<CandidateStatus>> SupplierTransitions = new()
    {
        [CandidateStatus.Received] = [CandidateStatus.UnderReview, CandidateStatus.Cancelled],
        [CandidateStatus.UnderReview] = [CandidateStatus.Approved, CandidateStatus.Rejected, CandidateStatus.FailedMedicalAbroad, CandidateStatus.Cancelled],
        [CandidateStatus.Approved] = [CandidateStatus.ProcurementPaid, CandidateStatus.FailedMedicalAbroad, CandidateStatus.VisaDenied, CandidateStatus.Rejected, CandidateStatus.Cancelled],
        [CandidateStatus.ProcurementPaid] = [CandidateStatus.InTransit, CandidateStatus.Cancelled],
        [CandidateStatus.InTransit] = [CandidateStatus.Arrived, CandidateStatus.Cancelled],
        [CandidateStatus.Arrived] = [CandidateStatus.Converted, CandidateStatus.ReturnedAfterArrival, CandidateStatus.Cancelled],
    };

    /// <summary>
    /// Valid transitions for local/walk-in candidates (short pipeline).
    /// </summary>
    private static readonly Dictionary<CandidateStatus, HashSet<CandidateStatus>> LocalTransitions = new()
    {
        [CandidateStatus.Received] = [CandidateStatus.UnderReview, CandidateStatus.Cancelled],
        [CandidateStatus.UnderReview] = [CandidateStatus.Approved, CandidateStatus.Rejected, CandidateStatus.Cancelled],
        [CandidateStatus.Approved] = [CandidateStatus.Converted, CandidateStatus.Rejected, CandidateStatus.Cancelled],
    };

    /// <summary>
    /// Statuses that require a reason when transitioning to them.
    /// </summary>
    private static readonly HashSet<CandidateStatus> ReasonRequired =
    [
        CandidateStatus.Rejected,
        CandidateStatus.Cancelled,
        CandidateStatus.FailedMedicalAbroad,
        CandidateStatus.VisaDenied,
        CandidateStatus.ReturnedAfterArrival,
    ];

    /// <summary>
    /// Validates a status transition.
    /// </summary>
    /// <param name="sourceType">The candidate's source type.</param>
    /// <param name="from">Current status.</param>
    /// <param name="to">Target status.</param>
    /// <param name="reason">Reason for the transition.</param>
    /// <returns>Null if valid; error message string if invalid.</returns>
    public static string? Validate(CandidateSourceType sourceType, CandidateStatus from, CandidateStatus to, string? reason)
    {
        var transitions = sourceType == CandidateSourceType.Supplier
            ? SupplierTransitions
            : LocalTransitions;

        if (!transitions.TryGetValue(from, out var validTargets))
            return $"Status '{from}' is a terminal status and cannot be transitioned";

        if (!validTargets.Contains(to))
            return $"Transition from '{from}' to '{to}' is not allowed for {sourceType} candidates";

        if (ReasonRequired.Contains(to) && string.IsNullOrWhiteSpace(reason))
            return $"A reason is required when transitioning to '{to}'";

        return null;
    }
}
