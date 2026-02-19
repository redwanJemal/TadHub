using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace TadHub.Infrastructure.Sse;

/// <summary>
/// In-memory implementation of SSE connection manager.
/// For multi-instance deployments, use Redis Pub/Sub via ISseNotifier.
/// </summary>
public sealed class SseConnectionManager : ISseConnectionManager
{
    private readonly ConcurrentDictionary<string, SseConnection> _connections = new();
    private readonly ILogger<SseConnectionManager> _logger;

    public SseConnectionManager(ILogger<SseConnectionManager> logger)
    {
        _logger = logger;
    }

    public int ConnectionCount => _connections.Count;

    public void AddConnection(SseConnection connection)
    {
        if (_connections.TryAdd(connection.ConnectionId, connection))
        {
            _logger.LogDebug(
                "SSE connection added: {ConnectionId} for user {UserId} in tenant {TenantId}",
                connection.ConnectionId, connection.UserId, connection.TenantId);
        }
    }

    public void RemoveConnection(string connectionId)
    {
        if (_connections.TryRemove(connectionId, out var connection))
        {
            _logger.LogDebug(
                "SSE connection removed: {ConnectionId} for user {UserId}",
                connectionId, connection.UserId);
        }
    }

    public IEnumerable<SseConnection> GetConnectionsByUser(Guid userId)
    {
        return _connections.Values.Where(c => c.UserId == userId);
    }

    public IEnumerable<SseConnection> GetConnectionsByTenant(Guid tenantId)
    {
        return _connections.Values.Where(c => c.TenantId == tenantId);
    }

    public IEnumerable<SseConnection> GetAllConnections()
    {
        return _connections.Values;
    }
}
