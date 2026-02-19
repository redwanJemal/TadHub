using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace TadHub.Infrastructure.Sse;

/// <summary>
/// Represents an active SSE connection to a client.
/// </summary>
public sealed class SseConnection : IAsyncDisposable
{
    private readonly HttpResponse _response;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private bool _isDisposed;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Unique identifier for this connection.
    /// </summary>
    public string ConnectionId { get; }

    /// <summary>
    /// The user ID associated with this connection.
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// The tenant ID associated with this connection.
    /// </summary>
    public Guid TenantId { get; }

    /// <summary>
    /// When this connection was established.
    /// </summary>
    public DateTimeOffset ConnectedAt { get; }

    /// <summary>
    /// Last activity timestamp.
    /// </summary>
    public DateTimeOffset LastActivity { get; private set; }

    public SseConnection(HttpResponse response, Guid userId, Guid tenantId)
    {
        ConnectionId = Guid.NewGuid().ToString("N")[..12];
        _response = response;
        UserId = userId;
        TenantId = tenantId;
        ConnectedAt = DateTimeOffset.UtcNow;
        LastActivity = ConnectedAt;
    }

    /// <summary>
    /// Writes an SSE event to the client.
    /// </summary>
    /// <param name="eventType">The event type (e.g., "message", "notification").</param>
    /// <param name="data">The event data (will be JSON serialized).</param>
    /// <param name="id">Optional event ID for client reconnection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task WriteEventAsync<T>(
        string eventType,
        T data,
        string? id = null,
        CancellationToken cancellationToken = default)
    {
        if (_isDisposed)
            return;

        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            var json = JsonSerializer.Serialize(data, JsonOptions);

            if (!string.IsNullOrEmpty(id))
            {
                await _response.WriteAsync($"id: {id}\n", cancellationToken);
            }

            await _response.WriteAsync($"event: {eventType}\n", cancellationToken);
            await _response.WriteAsync($"data: {json}\n\n", cancellationToken);
            await _response.Body.FlushAsync(cancellationToken);

            LastActivity = DateTimeOffset.UtcNow;
        }
        finally
        {
            _writeLock.Release();
        }
    }

    /// <summary>
    /// Writes a comment (keepalive) to the client.
    /// </summary>
    public async Task WriteCommentAsync(string comment, CancellationToken cancellationToken = default)
    {
        if (_isDisposed)
            return;

        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            await _response.WriteAsync($": {comment}\n\n", cancellationToken);
            await _response.Body.FlushAsync(cancellationToken);
            LastActivity = DateTimeOffset.UtcNow;
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        _writeLock.Dispose();
        await Task.CompletedTask;
    }
}
