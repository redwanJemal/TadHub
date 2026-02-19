using Worker.Core.Entities;

namespace Worker.Core.StateMachine;

/// <summary>
/// Exception thrown when an invalid state transition is attempted.
/// Maps to HTTP 409 Conflict via GlobalExceptionHandler.
/// </summary>
public class InvalidStateTransitionException : Exception
{
    /// <summary>
    /// The state the worker is currently in.
    /// </summary>
    public WorkerStatus FromStatus { get; }

    /// <summary>
    /// The attempted target state.
    /// </summary>
    public WorkerStatus ToStatus { get; }

    /// <summary>
    /// Error code for programmatic handling.
    /// </summary>
    public string ErrorCode { get; }

    public InvalidStateTransitionException(
        WorkerStatus from,
        WorkerStatus to,
        string? message = null,
        string? errorCode = null)
        : base(message ?? $"Invalid state transition from {from} to {to}")
    {
        FromStatus = from;
        ToStatus = to;
        ErrorCode = errorCode ?? "INVALID_STATE_TRANSITION";
    }

    public InvalidStateTransitionException(
        WorkerStatus from,
        WorkerStatus to,
        string message,
        Exception innerException)
        : base(message, innerException)
    {
        FromStatus = from;
        ToStatus = to;
        ErrorCode = "INVALID_STATE_TRANSITION";
    }
}
