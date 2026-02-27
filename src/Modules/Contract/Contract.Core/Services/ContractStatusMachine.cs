using Contract.Core.Entities;

namespace Contract.Core.Services;

public static class ContractStatusMachine
{
    private static readonly Dictionary<ContractStatus, HashSet<ContractStatus>> Transitions = new()
    {
        [ContractStatus.Draft] = [ContractStatus.Confirmed, ContractStatus.Cancelled],
        [ContractStatus.Confirmed] = [ContractStatus.OnProbation, ContractStatus.Active, ContractStatus.Cancelled],
        [ContractStatus.OnProbation] = [ContractStatus.Active, ContractStatus.Terminated, ContractStatus.Cancelled],
        [ContractStatus.Active] = [ContractStatus.Completed, ContractStatus.Terminated],
        [ContractStatus.Completed] = [ContractStatus.Closed],
        [ContractStatus.Terminated] = [ContractStatus.Closed],
        // Terminal states — no transitions
        // Cancelled, Closed are not in the dictionary
    };

    private static readonly HashSet<ContractStatus> ReasonRequired =
    [
        ContractStatus.Terminated,
        ContractStatus.Cancelled,
    ];

    private static readonly HashSet<ContractStatus> TerminalStatuses =
    [
        ContractStatus.Cancelled,
        ContractStatus.Closed,
    ];

    public static string? Validate(ContractStatus from, ContractStatus to, string? reason)
    {
        if (!Transitions.TryGetValue(from, out var validTargets))
            return $"Status '{from}' is a terminal status and cannot be transitioned";

        if (!validTargets.Contains(to))
            return $"Transition from '{from}' to '{to}' is not allowed";

        if (ReasonRequired.Contains(to) && string.IsNullOrWhiteSpace(reason))
            return $"A reason is required when transitioning to '{to}'";

        return null;
    }

    public static IReadOnlyList<ContractStatus> GetAllowedTransitions(ContractStatus from)
    {
        if (Transitions.TryGetValue(from, out var targets))
            return targets.ToList();

        return [];
    }

    public static bool IsReasonRequired(ContractStatus status) => ReasonRequired.Contains(status);

    public static bool IsTerminal(ContractStatus status) => TerminalStatuses.Contains(status);
}
