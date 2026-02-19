namespace TadHub.SharedKernel.Api;

/// <summary>
/// Standard API response envelope for single resources.
/// </summary>
/// <typeparam name="T">The type of the data payload.</typeparam>
public sealed class ApiResponse<T>
{
    /// <summary>
    /// The response data payload.
    /// </summary>
    public T Data { get; init; } = default!;

    /// <summary>
    /// Response metadata.
    /// </summary>
    public ApiMeta Meta { get; init; } = new();

    /// <summary>
    /// Creates a successful response with the given data.
    /// </summary>
    public static ApiResponse<T> Ok(T data, string? requestId = null) => new()
    {
        Data = data,
        Meta = ApiMeta.Create(requestId)
    };

    /// <summary>
    /// Creates a successful response for resource creation.
    /// </summary>
    public static ApiResponse<T> Created(T data, string? requestId = null) => Ok(data, requestId);
}

/// <summary>
/// Response metadata included in all API responses.
/// </summary>
public sealed class ApiMeta
{
    /// <summary>
    /// UTC timestamp when the response was generated.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; }

    /// <summary>
    /// Unique identifier for request tracing.
    /// </summary>
    public string RequestId { get; init; } = string.Empty;

    /// <summary>
    /// Creates metadata with current timestamp.
    /// </summary>
    public static ApiMeta Create(string? requestId = null) => new()
    {
        Timestamp = DateTimeOffset.UtcNow,
        RequestId = requestId ?? Guid.NewGuid().ToString("N")[..12]
    };
}
