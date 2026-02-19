using TadHub.SharedKernel.Models;
using Worker.Core.Entities;

namespace Worker.Core.StateMachine;

/// <summary>
/// Validates state transitions with full precondition checks.
/// </summary>
public class StateTransitionValidator
{
    /// <summary>
    /// Validates a transition and returns detailed result.
    /// </summary>
    public Result<TransitionRule> ValidateTransition(
        WorkerStatus from,
        WorkerStatus to,
        TransitionContext context)
    {
        // Check if transition is valid at all
        if (!WorkerStateMachine.IsValidTransition(from, to))
        {
            return Result<TransitionRule>.Failure(
                $"Invalid transition from {from} to {to}",
                "INVALID_TRANSITION");
        }

        // Get the rule
        var rule = WorkerStateMachine.GetTransitionRule(from, to);
        if (rule == null)
        {
            return Result<TransitionRule>.Failure(
                $"No rule found for transition {from} -> {to}",
                "NO_RULE_FOUND");
        }

        // Check preconditions
        var (isValid, failureReason) = rule.Precondition(context);
        if (!isValid)
        {
            return Result<TransitionRule>.Failure(
                failureReason ?? "Precondition failed",
                "PRECONDITION_FAILED");
        }

        return Result<TransitionRule>.Success(rule);
    }

    /// <summary>
    /// Validates a complex transition (Booked -> Hired) with all required checks.
    /// </summary>
    public Result<TransitionRule> ValidateBookedToHired(TransitionContext context)
    {
        var errors = new List<string>();

        if (!context.HasValidMedical)
            errors.Add("Valid medical clearance required");

        if (!context.HasValidVisa)
            errors.Add("Valid visa required");

        if (!context.HasActiveInsurance)
            errors.Add("Active insurance required");

        if (!context.IsClientVerified)
            errors.Add("Client must be verified");

        if (!context.RelatedEntityId.HasValue)
            errors.Add("Contract ID required");

        if (errors.Count > 0)
        {
            return Result<TransitionRule>.Failure(
                string.Join("; ", errors),
                "PRECONDITIONS_NOT_MET");
        }

        return Result<TransitionRule>.Success(
            new TransitionRule(_ => (true, null), "Contract signed, ready for deployment"));
    }
}
