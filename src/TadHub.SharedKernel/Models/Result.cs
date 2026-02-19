namespace TadHub.SharedKernel.Models;

/// <summary>
/// Represents the result of an operation that can either succeed with a value or fail with an error.
/// Use this instead of throwing exceptions for expected failure cases.
/// </summary>
/// <typeparam name="T">The type of the value on success.</typeparam>
public sealed class Result<T>
{
    /// <summary>
    /// Indicates whether the operation succeeded.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Indicates whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// The value if the operation succeeded. Null if failed.
    /// </summary>
    public T? Value { get; }

    /// <summary>
    /// The error message if the operation failed. Null if succeeded.
    /// </summary>
    public string? Error { get; }

    /// <summary>
    /// Optional error code for programmatic handling.
    /// </summary>
    public string? ErrorCode { get; }

    private Result(bool isSuccess, T? value, string? error, string? errorCode)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Creates a successful result with the given value.
    /// </summary>
    public static Result<T> Success(T value) => new(true, value, null, null);

    /// <summary>
    /// Creates a failed result with the given error message.
    /// </summary>
    public static Result<T> Failure(string error, string? code = null) => new(false, default, error, code);

    /// <summary>
    /// Creates a "not found" failure result.
    /// </summary>
    public static Result<T> NotFound(string? message = null) =>
        Failure(message ?? "Resource not found", "NOT_FOUND");

    /// <summary>
    /// Creates a "validation error" failure result.
    /// </summary>
    public static Result<T> ValidationError(string message) =>
        Failure(message, "VALIDATION_ERROR");

    /// <summary>
    /// Creates a "conflict" failure result (e.g., duplicate).
    /// </summary>
    public static Result<T> Conflict(string message) =>
        Failure(message, "CONFLICT");

    /// <summary>
    /// Creates an "unauthorized" failure result.
    /// </summary>
    public static Result<T> Unauthorized(string? message = null) =>
        Failure(message ?? "Unauthorized", "UNAUTHORIZED");

    /// <summary>
    /// Creates a "forbidden" failure result.
    /// </summary>
    public static Result<T> Forbidden(string? message = null) =>
        Failure(message ?? "Forbidden", "FORBIDDEN");

    /// <summary>
    /// Maps the value to a new type if successful.
    /// </summary>
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
    {
        return IsSuccess
            ? Result<TNew>.Success(mapper(Value!))
            : Result<TNew>.Failure(Error!, ErrorCode);
    }

    /// <summary>
    /// Implicit conversion to bool (true if success).
    /// </summary>
    public static implicit operator bool(Result<T> result) => result.IsSuccess;
}

/// <summary>
/// Non-generic result for operations that don't return a value.
/// </summary>
public sealed class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public string? ErrorCode { get; }

    private Result(bool isSuccess, string? error, string? errorCode)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true, null, null);
    public static Result Failure(string error, string? code = null) => new(false, error, code);
    public static Result NotFound(string? message = null) => Failure(message ?? "Resource not found", "NOT_FOUND");
    public static Result ValidationError(string message) => Failure(message, "VALIDATION_ERROR");
    public static Result Conflict(string message) => Failure(message, "CONFLICT");

    public static implicit operator bool(Result result) => result.IsSuccess;
}
