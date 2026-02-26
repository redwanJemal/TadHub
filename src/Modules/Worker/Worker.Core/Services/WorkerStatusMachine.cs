using Worker.Core.Entities;

namespace Worker.Core.Services;

/// <summary>
/// Validates worker status transitions.
/// </summary>
public static class WorkerStatusMachine
{
    private static readonly Dictionary<WorkerStatus, HashSet<WorkerStatus>> Transitions = new()
    {
        [WorkerStatus.Active] = [WorkerStatus.Deployed, WorkerStatus.OnLeave, WorkerStatus.Terminated],
        [WorkerStatus.Deployed] = [WorkerStatus.Active, WorkerStatus.OnLeave, WorkerStatus.Terminated],
        [WorkerStatus.OnLeave] = [WorkerStatus.Active, WorkerStatus.Deployed, WorkerStatus.Terminated],
        // Terminated is terminal — no transitions allowed
    };

    private static readonly HashSet<WorkerStatus> ReasonRequired =
    [
        WorkerStatus.Terminated,
        WorkerStatus.OnLeave,
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
}
