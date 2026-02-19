using System.Text.Json.Serialization;

namespace SaasKit.SharedKernel.Api;

/// <summary>
/// RFC 9457 Problem Details response for API errors.
/// </summary>
public sealed class ApiError
{
    private const string BaseTypeUri = "https://api.saaskit.dev/errors";

    /// <summary>
    /// A URI reference that identifies the problem type.
    /// </summary>
    public string Type { get; init; } = "about:blank";

    /// <summary>
    /// A short, human-readable summary of the problem type.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// The HTTP status code.
    /// </summary>
    public int Status { get; init; }

    /// <summary>
    /// A human-readable explanation specific to this occurrence.
    /// </summary>
    public string Detail { get; init; } = string.Empty;

    /// <summary>
    /// A URI reference that identifies the specific occurrence.
    /// </summary>
    public string? Instance { get; init; }

    /// <summary>
    /// Request ID for tracing.
    /// </summary>
    public string RequestId { get; init; } = string.Empty;

    /// <summary>
    /// Validation errors (field name → error messages).
    /// Only present for 422 Unprocessable Entity responses.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, string[]>? Errors { get; init; }

    /// <summary>
    /// Creates a 400 Bad Request error.
    /// </summary>
    public static ApiError BadRequest(string detail, string? instance = null) => new()
    {
        Type = $"{BaseTypeUri}/bad-request",
        Title = "Bad Request",
        Status = 400,
        Detail = detail,
        Instance = instance,
        RequestId = GenerateRequestId()
    };

    /// <summary>
    /// Creates a 401 Unauthorized error.
    /// </summary>
    public static ApiError Unauthorized(string? detail = null) => new()
    {
        Type = $"{BaseTypeUri}/unauthorized",
        Title = "Unauthorized",
        Status = 401,
        Detail = detail ?? "Authentication is required to access this resource.",
        RequestId = GenerateRequestId()
    };

    /// <summary>
    /// Creates a 403 Forbidden error.
    /// </summary>
    public static ApiError Forbidden(string? detail = null) => new()
    {
        Type = $"{BaseTypeUri}/forbidden",
        Title = "Forbidden",
        Status = 403,
        Detail = detail ?? "You don't have permission to access this resource.",
        RequestId = GenerateRequestId()
    };

    /// <summary>
    /// Creates a 404 Not Found error.
    /// </summary>
    public static ApiError NotFound(string detail, string? instance = null) => new()
    {
        Type = $"{BaseTypeUri}/not-found",
        Title = "Not Found",
        Status = 404,
        Detail = detail,
        Instance = instance,
        RequestId = GenerateRequestId()
    };

    /// <summary>
    /// Creates a 409 Conflict error.
    /// </summary>
    public static ApiError Conflict(string detail, string? instance = null) => new()
    {
        Type = $"{BaseTypeUri}/conflict",
        Title = "Conflict",
        Status = 409,
        Detail = detail,
        Instance = instance,
        RequestId = GenerateRequestId()
    };

    /// <summary>
    /// Creates a 422 Unprocessable Entity error with validation errors.
    /// </summary>
    public static ApiError Validation(Dictionary<string, string[]> errors, string? instance = null) => new()
    {
        Type = $"{BaseTypeUri}/validation",
        Title = "Validation Failed",
        Status = 422,
        Detail = "One or more validation errors occurred.",
        Instance = instance,
        RequestId = GenerateRequestId(),
        Errors = errors
    };

    /// <summary>
    /// Creates a 429 Too Many Requests error.
    /// </summary>
    public static ApiError TooManyRequests(string? detail = null) => new()
    {
        Type = $"{BaseTypeUri}/too-many-requests",
        Title = "Too Many Requests",
        Status = 429,
        Detail = detail ?? "Rate limit exceeded. Please try again later.",
        RequestId = GenerateRequestId()
    };

    /// <summary>
    /// Creates a 500 Internal Server Error.
    /// </summary>
    public static ApiError Internal(string? detail = null) => new()
    {
        Type = $"{BaseTypeUri}/internal-error",
        Title = "Internal Server Error",
        Status = 500,
        Detail = detail ?? "An unexpected error occurred.",
        RequestId = GenerateRequestId()
    };

    private static string GenerateRequestId() => Guid.NewGuid().ToString("N")[..12];
}
