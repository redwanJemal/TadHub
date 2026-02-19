namespace SaasKit.Infrastructure.Sse;

/// <summary>
/// Service for sending SSE notifications to connected clients.
/// Supports multi-instance deployments via Redis Pub/Sub.
/// </summary>
public interface ISseNotifier
{
    /// <summary>
    /// Sends an event to a specific user (all their connections).
    /// </summary>
    /// <typeparam name="T">The event data type.</typeparam>
    /// <param name="userId">The target user ID.</param>
    /// <param name="eventType">The event type name.</param>
    /// <param name="data">The event data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendToUserAsync<T>(
        Guid userId,
        string eventType,
        T data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an event to all users in a tenant.
    /// </summary>
    /// <typeparam name="T">The event data type.</typeparam>
    /// <param name="tenantId">The target tenant ID.</param>
    /// <param name="eventType">The event type name.</param>
    /// <param name="data">The event data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendToTenantAsync<T>(
        Guid tenantId,
        string eventType,
        T data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an event to all connected users (broadcast).
    /// </summary>
    /// <typeparam name="T">The event data type.</typeparam>
    /// <param name="eventType">The event type name.</param>
    /// <param name="data">The event data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task BroadcastAsync<T>(
        string eventType,
        T data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an event to specific connections.
    /// </summary>
    /// <typeparam name="T">The event data type.</typeparam>
    /// <param name="connectionIds">The target connection IDs.</param>
    /// <param name="eventType">The event type name.</param>
    /// <param name="data">The event data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendToConnectionsAsync<T>(
        IEnumerable<string> connectionIds,
        string eventType,
        T data,
        CancellationToken cancellationToken = default);
}
