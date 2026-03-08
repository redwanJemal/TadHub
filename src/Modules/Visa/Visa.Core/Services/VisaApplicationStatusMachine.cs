using Visa.Core.Entities;

namespace Visa.Core.Services;

public static class VisaApplicationStatusMachine
{
    private static readonly Dictionary<VisaApplicationStatus, HashSet<VisaApplicationStatus>> Transitions = new()
    {
        [VisaApplicationStatus.NotStarted] = [VisaApplicationStatus.DocumentsCollecting, VisaApplicationStatus.Applied, VisaApplicationStatus.Cancelled],
        [VisaApplicationStatus.DocumentsCollecting] = [VisaApplicationStatus.Applied, VisaApplicationStatus.Cancelled],
        [VisaApplicationStatus.Applied] = [VisaApplicationStatus.UnderProcess, VisaApplicationStatus.Rejected, VisaApplicationStatus.Cancelled],
        [VisaApplicationStatus.UnderProcess] = [VisaApplicationStatus.Approved, VisaApplicationStatus.Rejected, VisaApplicationStatus.Cancelled],
        [VisaApplicationStatus.Approved] = [VisaApplicationStatus.Issued, VisaApplicationStatus.Cancelled],
        [VisaApplicationStatus.Rejected] = [VisaApplicationStatus.Applied, VisaApplicationStatus.Cancelled],
        [VisaApplicationStatus.Issued] = [VisaApplicationStatus.Expired],
    };

    private static readonly HashSet<VisaApplicationStatus> ReasonRequired =
    [
        VisaApplicationStatus.Rejected,
        VisaApplicationStatus.Cancelled,
    ];

    private static readonly HashSet<VisaApplicationStatus> TerminalStatuses =
    [
        VisaApplicationStatus.Expired,
        VisaApplicationStatus.Cancelled,
    ];

    public static string? Validate(VisaApplicationStatus from, VisaApplicationStatus to, string? reason)
    {
        if (!Transitions.TryGetValue(from, out var validTargets))
            return $"Status '{from}' is a terminal status and cannot be transitioned";

        if (!validTargets.Contains(to))
            return $"Transition from '{from}' to '{to}' is not allowed";

        if (ReasonRequired.Contains(to) && string.IsNullOrWhiteSpace(reason))
            return $"A reason is required when transitioning to '{to}'";

        return null;
    }

    public static IReadOnlyList<VisaApplicationStatus> GetAllowedTransitions(VisaApplicationStatus from)
    {
        if (Transitions.TryGetValue(from, out var targets))
            return targets.ToList();
        return [];
    }

    public static bool IsReasonRequired(VisaApplicationStatus status) => ReasonRequired.Contains(status);
    public static bool IsTerminal(VisaApplicationStatus status) => TerminalStatuses.Contains(status);
}
