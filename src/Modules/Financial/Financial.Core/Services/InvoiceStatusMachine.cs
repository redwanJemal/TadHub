using Financial.Core.Entities;

namespace Financial.Core.Services;

public static class InvoiceStatusMachine
{
    private static readonly Dictionary<InvoiceStatus, HashSet<InvoiceStatus>> Transitions = new()
    {
        [InvoiceStatus.Draft] = [InvoiceStatus.Issued, InvoiceStatus.Cancelled],
        [InvoiceStatus.Issued] = [InvoiceStatus.PartiallyPaid, InvoiceStatus.Paid, InvoiceStatus.Overdue, InvoiceStatus.Cancelled],
        [InvoiceStatus.PartiallyPaid] = [InvoiceStatus.Paid, InvoiceStatus.Overdue, InvoiceStatus.Cancelled],
        [InvoiceStatus.Overdue] = [InvoiceStatus.PartiallyPaid, InvoiceStatus.Paid, InvoiceStatus.Cancelled],
        [InvoiceStatus.Paid] = [InvoiceStatus.Refunded],
        // Terminal states — Cancelled, Refunded are not in the dictionary
    };

    private static readonly HashSet<InvoiceStatus> ReasonRequired =
    [
        InvoiceStatus.Cancelled,
        InvoiceStatus.Refunded,
    ];

    private static readonly HashSet<InvoiceStatus> TerminalStatuses =
    [
        InvoiceStatus.Cancelled,
        InvoiceStatus.Refunded,
    ];

    public static string? Validate(InvoiceStatus from, InvoiceStatus to, string? reason)
    {
        if (!Transitions.TryGetValue(from, out var validTargets))
            return $"Status '{from}' is a terminal status and cannot be transitioned";

        if (!validTargets.Contains(to))
            return $"Transition from '{from}' to '{to}' is not allowed";

        if (ReasonRequired.Contains(to) && string.IsNullOrWhiteSpace(reason))
            return $"A reason is required when transitioning to '{to}'";

        return null;
    }

    public static IReadOnlyList<InvoiceStatus> GetAllowedTransitions(InvoiceStatus from)
    {
        if (Transitions.TryGetValue(from, out var targets))
            return targets.ToList();

        return [];
    }

    public static bool IsReasonRequired(InvoiceStatus status) => ReasonRequired.Contains(status);

    public static bool IsTerminal(InvoiceStatus status) => TerminalStatuses.Contains(status);
}
