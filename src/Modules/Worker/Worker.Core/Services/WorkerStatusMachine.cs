using Worker.Core.Entities;

namespace Worker.Core.Services;

/// <summary>
/// Validates worker status transitions across the 18-state lifecycle.
/// </summary>
public static class WorkerStatusMachine
{
    private static readonly Dictionary<WorkerStatus, HashSet<WorkerStatus>> Transitions = new()
    {
        [WorkerStatus.Available] = [WorkerStatus.Booked, WorkerStatus.UnderMedicalTest, WorkerStatus.InTraining, WorkerStatus.Absconded, WorkerStatus.Repatriated, WorkerStatus.Deceased],
        [WorkerStatus.InTraining] = [WorkerStatus.Available, WorkerStatus.UnderMedicalTest, WorkerStatus.Absconded, WorkerStatus.Repatriated, WorkerStatus.Deceased],
        [WorkerStatus.UnderMedicalTest] = [WorkerStatus.Available, WorkerStatus.MedicallyUnfit, WorkerStatus.Deceased],
        [WorkerStatus.NewArrival] = [WorkerStatus.Available, WorkerStatus.InTraining, WorkerStatus.UnderMedicalTest, WorkerStatus.MedicallyUnfit, WorkerStatus.Absconded, WorkerStatus.Repatriated, WorkerStatus.Deceased],
        [WorkerStatus.Booked] = [WorkerStatus.Hired, WorkerStatus.NewArrival, WorkerStatus.Available, WorkerStatus.Deceased],
        [WorkerStatus.Hired] = [WorkerStatus.OnProbation, WorkerStatus.Available, WorkerStatus.Deceased],
        [WorkerStatus.OnProbation] = [WorkerStatus.Active, WorkerStatus.PendingReplacement, WorkerStatus.Terminated, WorkerStatus.Absconded, WorkerStatus.Pregnant, WorkerStatus.Deceased],
        [WorkerStatus.Active] = [WorkerStatus.Renewed, WorkerStatus.PendingReplacement, WorkerStatus.Terminated, WorkerStatus.Absconded, WorkerStatus.Pregnant, WorkerStatus.Transferred, WorkerStatus.Deceased],
        [WorkerStatus.Renewed] = [WorkerStatus.Active, WorkerStatus.PendingReplacement, WorkerStatus.Terminated, WorkerStatus.Absconded, WorkerStatus.Pregnant, WorkerStatus.Transferred, WorkerStatus.Deceased],
        [WorkerStatus.PendingReplacement] = [WorkerStatus.Available, WorkerStatus.Terminated, WorkerStatus.Repatriated, WorkerStatus.Deceased],
        [WorkerStatus.Transferred] = [WorkerStatus.Repatriated],
        [WorkerStatus.MedicallyUnfit] = [WorkerStatus.Repatriated, WorkerStatus.Available, WorkerStatus.Deceased],
        [WorkerStatus.Absconded] = [WorkerStatus.Terminated, WorkerStatus.Repatriated, WorkerStatus.Deported, WorkerStatus.Available, WorkerStatus.Deceased],
        [WorkerStatus.Terminated] = [WorkerStatus.Available, WorkerStatus.Repatriated, WorkerStatus.Transferred],
        [WorkerStatus.Pregnant] = [WorkerStatus.Active, WorkerStatus.Terminated, WorkerStatus.Repatriated, WorkerStatus.Deceased],
        // Terminal states — no transitions allowed
        // Repatriated, Deported, Deceased are not in the dictionary
    };

    private static readonly HashSet<WorkerStatus> ReasonRequired =
    [
        WorkerStatus.Terminated,
        WorkerStatus.Absconded,
        WorkerStatus.MedicallyUnfit,
        WorkerStatus.PendingReplacement,
        WorkerStatus.Transferred,
        WorkerStatus.Repatriated,
        WorkerStatus.Deported,
        WorkerStatus.Pregnant,
        WorkerStatus.Deceased,
    ];

    private static readonly HashSet<WorkerStatus> TerminalStatuses =
    [
        WorkerStatus.Repatriated,
        WorkerStatus.Deported,
        WorkerStatus.Deceased,
    ];

    /// <summary>
    /// Validates a status transition.
    /// </summary>
    /// <returns>Null if valid; error message string if invalid.</returns>
    public static string? Validate(WorkerStatus from, WorkerStatus to, string? reason)
    {
        if (!Transitions.TryGetValue(from, out var validTargets))
            return $"Status '{from}' is a terminal status and cannot be transitioned";

        if (!validTargets.Contains(to))
            return $"Transition from '{from}' to '{to}' is not allowed";

        if (ReasonRequired.Contains(to) && string.IsNullOrWhiteSpace(reason))
            return $"A reason is required when transitioning to '{to}'";

        return null;
    }

    /// <summary>
    /// Returns the list of statuses reachable from the given status.
    /// </summary>
    public static IReadOnlyList<WorkerStatus> GetAllowedTransitions(WorkerStatus from)
    {
        if (Transitions.TryGetValue(from, out var targets))
            return targets.ToList();

        return [];
    }

    /// <summary>
    /// Returns whether a reason is required for the given target status.
    /// </summary>
    public static bool IsReasonRequired(WorkerStatus status) => ReasonRequired.Contains(status);

    /// <summary>
    /// Returns whether the given status is terminal (no outgoing transitions).
    /// </summary>
    public static bool IsTerminal(WorkerStatus status) => TerminalStatuses.Contains(status);

    /// <summary>
    /// Returns the lifecycle category for a given status.
    /// </summary>
    public static WorkerStatusCategory GetCategory(WorkerStatus status) => status switch
    {
        WorkerStatus.Available or WorkerStatus.InTraining or WorkerStatus.UnderMedicalTest => WorkerStatusCategory.Pool,
        WorkerStatus.NewArrival => WorkerStatusCategory.Arrival,
        WorkerStatus.Booked or WorkerStatus.Hired or WorkerStatus.OnProbation or WorkerStatus.Active or WorkerStatus.Renewed => WorkerStatusCategory.Placement,
        WorkerStatus.PendingReplacement or WorkerStatus.Transferred or WorkerStatus.MedicallyUnfit or WorkerStatus.Absconded or WorkerStatus.Terminated or WorkerStatus.Pregnant => WorkerStatusCategory.NegativeSpecial,
        WorkerStatus.Repatriated or WorkerStatus.Deported or WorkerStatus.Deceased => WorkerStatusCategory.Terminal,
        _ => WorkerStatusCategory.Pool,
    };
}

public enum WorkerStatusCategory
{
    Pool,
    Arrival,
    Placement,
    NegativeSpecial,
    Terminal,
}
