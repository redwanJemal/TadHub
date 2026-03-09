using Placement.Core.Entities;

namespace Placement.Core.Services;

public static class PlacementStatusMachine
{
    // Outside-country 9-step flow:
    // 1. Booked → 2. ContractCreated → 3. EmploymentVisaProcessing → 4. TicketArranged
    // → 5. Arrived → 6. Deployed → 7. FullPaymentReceived → 8. ResidenceVisaProcessing
    // → 9. EmiratesIdProcessing → Completed
    private static readonly Dictionary<PlacementStatus, HashSet<PlacementStatus>> Transitions = new()
    {
        // Step 1 → Step 2
        [PlacementStatus.Booked] = [PlacementStatus.ContractCreated, PlacementStatus.Cancelled],
        // Step 2 → Step 3
        [PlacementStatus.ContractCreated] = [PlacementStatus.EmploymentVisaProcessing, PlacementStatus.Cancelled],
        // Step 3 → Step 4
        [PlacementStatus.EmploymentVisaProcessing] = [PlacementStatus.TicketArranged, PlacementStatus.Cancelled],
        // Step 4 → Step 5
        [PlacementStatus.TicketArranged] = [PlacementStatus.InTransit, PlacementStatus.Arrived, PlacementStatus.Cancelled],
        // Legacy: InTransit → Arrived
        [PlacementStatus.InTransit] = [PlacementStatus.Arrived, PlacementStatus.Cancelled],
        // Step 5 → Step 6
        [PlacementStatus.Arrived] = [PlacementStatus.Deployed, PlacementStatus.MedicalInProgress, PlacementStatus.Cancelled],
        // Legacy transitions (kept for backward compat)
        [PlacementStatus.MedicalInProgress] = [PlacementStatus.MedicalCleared, PlacementStatus.Cancelled],
        [PlacementStatus.MedicalCleared] = [PlacementStatus.GovtProcessing, PlacementStatus.Cancelled],
        [PlacementStatus.GovtProcessing] = [PlacementStatus.GovtCleared, PlacementStatus.Cancelled],
        [PlacementStatus.GovtCleared] = [PlacementStatus.Training, PlacementStatus.ReadyForPlacement, PlacementStatus.Cancelled],
        [PlacementStatus.Training] = [PlacementStatus.ReadyForPlacement, PlacementStatus.Cancelled],
        [PlacementStatus.ReadyForPlacement] = [PlacementStatus.Placed, PlacementStatus.Deployed, PlacementStatus.Cancelled],
        [PlacementStatus.Placed] = [PlacementStatus.Completed, PlacementStatus.Cancelled],
        // Step 6 → Step 7
        [PlacementStatus.Deployed] = [PlacementStatus.FullPaymentReceived, PlacementStatus.Cancelled],
        // Step 7 → Step 8
        [PlacementStatus.FullPaymentReceived] = [PlacementStatus.ResidenceVisaProcessing, PlacementStatus.Cancelled],
        // Step 8 → Step 9
        [PlacementStatus.ResidenceVisaProcessing] = [PlacementStatus.EmiratesIdProcessing, PlacementStatus.Cancelled],
        // Step 9 → Completed
        [PlacementStatus.EmiratesIdProcessing] = [PlacementStatus.Completed, PlacementStatus.Cancelled],
    };

    private static readonly HashSet<PlacementStatus> ReasonRequired =
    [
        PlacementStatus.Cancelled,
    ];

    private static readonly HashSet<PlacementStatus> TerminalStatuses =
    [
        PlacementStatus.Completed,
        PlacementStatus.Cancelled,
    ];

    /// <summary>
    /// The 9-step outside-country pipeline statuses in order.
    /// </summary>
    public static readonly PlacementStatus[] OutsideCountryPipeline =
    [
        PlacementStatus.Booked,
        PlacementStatus.ContractCreated,
        PlacementStatus.EmploymentVisaProcessing,
        PlacementStatus.TicketArranged,
        PlacementStatus.Arrived,
        PlacementStatus.Deployed,
        PlacementStatus.FullPaymentReceived,
        PlacementStatus.ResidenceVisaProcessing,
        PlacementStatus.EmiratesIdProcessing,
    ];

    public static string? Validate(PlacementStatus from, PlacementStatus to, string? reason)
    {
        if (!Transitions.TryGetValue(from, out var validTargets))
            return $"Status '{from}' is a terminal status and cannot be transitioned";

        if (!validTargets.Contains(to))
            return $"Transition from '{from}' to '{to}' is not allowed";

        if (ReasonRequired.Contains(to) && string.IsNullOrWhiteSpace(reason))
            return $"A reason is required when transitioning to '{to}'";

        return null;
    }

    public static IReadOnlyList<PlacementStatus> GetAllowedTransitions(PlacementStatus from)
    {
        if (Transitions.TryGetValue(from, out var targets))
            return targets.ToList();
        return [];
    }

    /// <summary>
    /// Gets the next step in the outside-country pipeline for the given current status.
    /// Returns null if current status is not in the pipeline or is the last step.
    /// </summary>
    public static PlacementStatus? GetNextOutsideCountryStep(PlacementStatus current)
    {
        var idx = Array.IndexOf(OutsideCountryPipeline, current);
        if (idx < 0 || idx >= OutsideCountryPipeline.Length - 1)
            return null;
        return OutsideCountryPipeline[idx + 1];
    }

    /// <summary>
    /// Gets the step number (1-based) for a status in the outside-country pipeline.
    /// Returns 0 if not in the pipeline.
    /// </summary>
    public static int GetStepNumber(PlacementStatus status)
    {
        var idx = Array.IndexOf(OutsideCountryPipeline, status);
        return idx >= 0 ? idx + 1 : 0;
    }

    public static bool IsReasonRequired(PlacementStatus status) => ReasonRequired.Contains(status);
    public static bool IsTerminal(PlacementStatus status) => TerminalStatuses.Contains(status);
}
