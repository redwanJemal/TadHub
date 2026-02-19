using Worker.Core.Entities;

namespace Worker.Core.StateMachine;

/// <summary>
/// Worker lifecycle state machine.
/// Defines valid transitions and rules for the 20-state FSM.
/// </summary>
public static class WorkerStateMachine
{
    /// <summary>
    /// Valid state transitions with their requirements.
    /// </summary>
    private static readonly Dictionary<(WorkerStatus From, WorkerStatus To), TransitionRule> Transitions = new()
    {
        // NewArrival transitions
        [(WorkerStatus.NewArrival, WorkerStatus.InTraining)] = new(RequireNone, "Worker started training"),
        [(WorkerStatus.NewArrival, WorkerStatus.UnderMedicalTest)] = new(RequireNone, "Sent for medical"),

        // InTraining transitions
        [(WorkerStatus.InTraining, WorkerStatus.ReadyForMarket)] = new(RequireMedicalClearance, "Training completed"),
        [(WorkerStatus.InTraining, WorkerStatus.UnderMedicalTest)] = new(RequireNone, "Sent for medical"),

        // UnderMedicalTest transitions
        [(WorkerStatus.UnderMedicalTest, WorkerStatus.ReadyForMarket)] = new(RequireMedicalClearance, "Medical cleared"),
        [(WorkerStatus.UnderMedicalTest, WorkerStatus.InTraining)] = new(RequireNone, "Back to training"),
        [(WorkerStatus.UnderMedicalTest, WorkerStatus.MedicallyUnfit)] = new(RequireNone, "Medical failed"),

        // ReadyForMarket transitions
        [(WorkerStatus.ReadyForMarket, WorkerStatus.Booked)] = new(RequireClientId, "Client booked worker"),
        [(WorkerStatus.ReadyForMarket, WorkerStatus.UnderMedicalTest)] = new(RequireNone, "Routine medical check"),

        // Booked transitions
        [(WorkerStatus.Booked, WorkerStatus.Hired)] = new(RequireContractId, "Contract signed"),
        [(WorkerStatus.Booked, WorkerStatus.ReadyForMarket)] = new(RequireNone, "Booking cancelled"),

        // Hired transitions
        [(WorkerStatus.Hired, WorkerStatus.AwaitingVisa)] = new(RequireNone, "Awaiting visa"),
        [(WorkerStatus.Hired, WorkerStatus.OnProbation)] = new(RequireNone, "Deployed to client"),

        // AwaitingVisa transitions
        [(WorkerStatus.AwaitingVisa, WorkerStatus.OnProbation)] = new(RequireNone, "Visa obtained, deployed"),
        [(WorkerStatus.AwaitingVisa, WorkerStatus.Terminated)] = new(RequireNone, "Visa rejected"),

        // OnProbation transitions
        [(WorkerStatus.OnProbation, WorkerStatus.Active)] = new(RequireNone, "Probation passed"),
        [(WorkerStatus.OnProbation, WorkerStatus.InProbationReview)] = new(RequireNone, "Probation issue"),
        [(WorkerStatus.OnProbation, WorkerStatus.PendingReplacement)] = new(RequireNone, "Client wants replacement"),
        [(WorkerStatus.OnProbation, WorkerStatus.Terminated)] = new(RequireNone, "Probation failed"),

        // InProbationReview transitions
        [(WorkerStatus.InProbationReview, WorkerStatus.OnProbation)] = new(RequireNone, "Issue resolved"),
        [(WorkerStatus.InProbationReview, WorkerStatus.Terminated)] = new(RequireNone, "Review failed"),
        [(WorkerStatus.InProbationReview, WorkerStatus.PendingReplacement)] = new(RequireNone, "Replacement requested"),

        // Active transitions
        [(WorkerStatus.Active, WorkerStatus.Renewed)] = new(RequireContractId, "Contract renewed"),
        [(WorkerStatus.Active, WorkerStatus.Transferred)] = new(RequireContractId, "Transferred to new employer"),
        [(WorkerStatus.Active, WorkerStatus.Terminated)] = new(RequireNone, "Contract terminated"),
        [(WorkerStatus.Active, WorkerStatus.PendingReplacement)] = new(RequireNone, "Replacement requested"),
        [(WorkerStatus.Active, WorkerStatus.Pregnant)] = new(RequireNone, "Worker is pregnant"),
        [(WorkerStatus.Active, WorkerStatus.UnderMedicalTest)] = new(RequireNone, "Medical check required"),

        // Renewed transitions
        [(WorkerStatus.Renewed, WorkerStatus.Active)] = new(RequireNone, "Renewal active"),
        [(WorkerStatus.Renewed, WorkerStatus.Terminated)] = new(RequireNone, "Renewal cancelled"),

        // Transferred transitions
        [(WorkerStatus.Transferred, WorkerStatus.OnProbation)] = new(RequireNone, "New probation period"),
        [(WorkerStatus.Transferred, WorkerStatus.Active)] = new(RequireNone, "Direct active with new employer"),

        // PendingReplacement transitions
        [(WorkerStatus.PendingReplacement, WorkerStatus.ReadyForMarket)] = new(RequireNone, "Available again"),
        [(WorkerStatus.PendingReplacement, WorkerStatus.Booked)] = new(RequireClientId, "Replacement booked"),
        [(WorkerStatus.PendingReplacement, WorkerStatus.Terminated)] = new(RequireNone, "Not replaced, terminated"),

        // Pregnant transitions
        [(WorkerStatus.Pregnant, WorkerStatus.Active)] = new(RequireNone, "Returned to work"),
        [(WorkerStatus.Pregnant, WorkerStatus.Repatriated)] = new(RequireNone, "Sent home"),
        [(WorkerStatus.Pregnant, WorkerStatus.Terminated)] = new(RequireNone, "Contract ended"),

        // MedicallyUnfit transitions
        [(WorkerStatus.MedicallyUnfit, WorkerStatus.Repatriated)] = new(RequireNone, "Sent home"),
        [(WorkerStatus.MedicallyUnfit, WorkerStatus.Terminated)] = new(RequireNone, "Contract ended"),

        // Terminated transitions
        [(WorkerStatus.Terminated, WorkerStatus.ReadyForMarket)] = new(RequireNone, "Re-available"),
        [(WorkerStatus.Terminated, WorkerStatus.Repatriated)] = new(RequireNone, "Sent home"),

        // Final states - no normal transitions out
        // But emergency transitions are always allowed (see IsEmergencyTransition)
    };

    /// <summary>
    /// Emergency transitions that can happen from ANY state.
    /// </summary>
    private static readonly HashSet<WorkerStatus> EmergencyTargetStates = new()
    {
        WorkerStatus.Absconded,
        WorkerStatus.Deported,
        WorkerStatus.Deceased
    };

    /// <summary>
    /// Terminal states (no transitions out except emergency).
    /// </summary>
    private static readonly HashSet<WorkerStatus> TerminalStates = new()
    {
        WorkerStatus.Absconded,
        WorkerStatus.Deported,
        WorkerStatus.Repatriated,
        WorkerStatus.Deceased
    };

    /// <summary>
    /// Checks if a transition is valid.
    /// </summary>
    public static bool IsValidTransition(WorkerStatus from, WorkerStatus to)
    {
        // Same state is not a transition
        if (from == to) return false;

        // Emergency transitions always allowed
        if (IsEmergencyTransition(to)) return true;

        // Terminal states can't transition out (except to other terminal via emergency)
        if (TerminalStates.Contains(from)) return false;

        // Check defined transitions
        return Transitions.ContainsKey((from, to));
    }

    /// <summary>
    /// Gets the transition rule if valid.
    /// </summary>
    public static TransitionRule? GetTransitionRule(WorkerStatus from, WorkerStatus to)
    {
        if (IsEmergencyTransition(to))
        {
            return new TransitionRule(RequireNone, $"Emergency: {to}");
        }

        return Transitions.TryGetValue((from, to), out var rule) ? rule : null;
    }

    /// <summary>
    /// Gets all valid target states from a given state.
    /// </summary>
    public static IEnumerable<WorkerStatus> GetValidTargetStates(WorkerStatus from)
    {
        // Emergency transitions
        foreach (var state in EmergencyTargetStates)
        {
            if (state != from)
                yield return state;
        }

        // Terminal states have no non-emergency transitions
        if (TerminalStates.Contains(from))
            yield break;

        // Regular transitions
        foreach (var ((f, t), _) in Transitions)
        {
            if (f == from)
                yield return t;
        }
    }

    /// <summary>
    /// Whether this is an emergency transition (allowed from any state).
    /// </summary>
    public static bool IsEmergencyTransition(WorkerStatus to) =>
        EmergencyTargetStates.Contains(to);

    /// <summary>
    /// Whether this is a terminal state.
    /// </summary>
    public static bool IsTerminalState(WorkerStatus status) =>
        TerminalStates.Contains(status);

    #region Precondition Checks

    private static TransitionPrecondition RequireNone => _ => (true, null);

    private static TransitionPrecondition RequireMedicalClearance => ctx =>
        ctx.HasValidMedical ? (true, null) : (false, "Valid medical clearance required");

    private static TransitionPrecondition RequireClientId => ctx =>
        ctx.RelatedEntityId.HasValue ? (true, null) : (false, "Client ID required");

    private static TransitionPrecondition RequireContractId => ctx =>
        ctx.RelatedEntityId.HasValue ? (true, null) : (false, "Contract ID required");

    #endregion
}

/// <summary>
/// Transition rule with preconditions.
/// </summary>
public record TransitionRule(
    TransitionPrecondition Precondition,
    string DefaultReason
);

/// <summary>
/// Precondition check for a transition.
/// </summary>
public delegate (bool IsValid, string? FailureReason) TransitionPrecondition(TransitionContext context);

/// <summary>
/// Context for evaluating transition preconditions.
/// </summary>
public record TransitionContext
{
    public Guid WorkerId { get; init; }
    public WorkerStatus CurrentStatus { get; init; }
    public WorkerStatus TargetStatus { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public bool HasValidMedical { get; init; }
    public bool HasValidVisa { get; init; }
    public bool HasActiveInsurance { get; init; }
    public bool IsClientVerified { get; init; }
}
