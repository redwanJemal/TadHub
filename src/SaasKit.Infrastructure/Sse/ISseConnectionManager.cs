namespace SaasKit.Infrastructure.Sse;

/// <summary>
/// Manages active SSE connections.
/// </summary>
public interface ISseConnectionManager
{
    /// <summary>
    /// Registers a new SSE connection.
    /// </summary>
    /// <param name="connection">The connection to register.</param>
    void AddConnection(SseConnection connection);

    /// <summary>
    /// Removes an SSE connection.
    /// </summary>
    /// <param name="connectionId">The connection ID to remove.</param>
    void RemoveConnection(string connectionId);

    /// <summary>
    /// Gets all connections for a specific user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>All connections for the user.</returns>
    IEnumerable<SseConnection> GetConnectionsByUser(Guid userId);

    /// <summary>
    /// Gets all connections for a specific tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID.</param>
    /// <returns>All connections for the tenant.</returns>
    IEnumerable<SseConnection> GetConnectionsByTenant(Guid tenantId);

    /// <summary>
    /// Gets all active connections.
    /// </summary>
    /// <returns>All active connections.</returns>
    IEnumerable<SseConnection> GetAllConnections();

    /// <summary>
    /// Gets the total number of active connections.
    /// </summary>
    int ConnectionCount { get; }
}
