using Financial.Core.Entities;

namespace Financial.Core.Services;

public static class PaymentStatusMachine
{
    private static readonly Dictionary<PaymentStatus, HashSet<PaymentStatus>> Transitions = new()
    {
        [PaymentStatus.Pending] = [PaymentStatus.Completed, PaymentStatus.Failed, PaymentStatus.Cancelled],
        [PaymentStatus.Failed] = [PaymentStatus.Pending],
        [PaymentStatus.Completed] = [PaymentStatus.Refunded],
        // Terminal states — Cancelled, Refunded are not in the dictionary
    };

    private static readonly HashSet<PaymentStatus> ReasonRequired =
    [
        PaymentStatus.Failed,
        PaymentStatus.Cancelled,
        PaymentStatus.Refunded,
    ];

    private static readonly HashSet<PaymentStatus> TerminalStatuses =
    [
        PaymentStatus.Cancelled,
        PaymentStatus.Refunded,
    ];

    public static string? Validate(PaymentStatus from, PaymentStatus to, string? reason)
    {
        if (!Transitions.TryGetValue(from, out var validTargets))
            return $"Status '{from}' is a terminal status and cannot be transitioned";

        if (!validTargets.Contains(to))
            return $"Transition from '{from}' to '{to}' is not allowed";

        if (ReasonRequired.Contains(to) && string.IsNullOrWhiteSpace(reason))
            return $"A reason is required when transitioning to '{to}'";

        return null;
    }

    public static IReadOnlyList<PaymentStatus> GetAllowedTransitions(PaymentStatus from)
    {
        if (Transitions.TryGetValue(from, out var targets))
            return targets.ToList();

        return [];
    }

    public static bool IsReasonRequired(PaymentStatus status) => ReasonRequired.Contains(status);

    public static bool IsTerminal(PaymentStatus status) => TerminalStatuses.Contains(status);
}
