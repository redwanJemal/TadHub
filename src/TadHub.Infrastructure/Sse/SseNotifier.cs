using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace TadHub.Infrastructure.Sse;

/// <summary>
/// SSE notifier implementation with Redis Pub/Sub for multi-instance support.
/// </summary>
public sealed class SseNotifier : ISseNotifier, IDisposable
{
    private readonly ISseConnectionManager _connectionManager;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<SseNotifier> _logger;
    private readonly ISubscriber _subscriber;
    private bool _isSubscribed;

    private const string UserChannel = "sse:user:";
    private const string TenantChannel = "sse:tenant:";
    private const string BroadcastChannel = "sse:broadcast";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public SseNotifier(
        ISseConnectionManager connectionManager,
        IConnectionMultiplexer redis,
        ILogger<SseNotifier> logger)
    {
        _connectionManager = connectionManager;
        _redis = redis;
        _logger = logger;
        _subscriber = _redis.GetSubscriber();

        SubscribeToChannels();
    }

    private void SubscribeToChannels()
    {
        if (_isSubscribed)
            return;

        try
        {
            // Subscribe to user-specific messages
            _subscriber.Subscribe(RedisChannel.Pattern($"{UserChannel}*"), async (channel, message) =>
            {
                await HandleUserMessage(channel!, message!);
            });

            // Subscribe to tenant-specific messages
            _subscriber.Subscribe(RedisChannel.Pattern($"{TenantChannel}*"), async (channel, message) =>
            {
                await HandleTenantMessage(channel!, message!);
            });

            // Subscribe to broadcast messages
            _subscriber.Subscribe(RedisChannel.Literal(BroadcastChannel), async (_, message) =>
            {
                await HandleBroadcastMessage(message!);
            });

            _isSubscribed = true;
            _logger.LogInformation("SSE notifier subscribed to Redis channels");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to Redis channels");
        }
    }

    public async Task SendToUserAsync<T>(
        Guid userId,
        string eventType,
        T data,
        CancellationToken cancellationToken = default)
    {
        var message = new SseMessage(eventType, JsonSerializer.Serialize(data, JsonOptions));
        
        // Publish to Redis for other instances
        await _subscriber.PublishAsync(
            RedisChannel.Literal($"{UserChannel}{userId}"),
            JsonSerializer.Serialize(message, JsonOptions));

        // Also send to local connections
        await SendToLocalUserAsync(userId, eventType, data, cancellationToken);
    }

    public async Task SendToTenantAsync<T>(
        Guid tenantId,
        string eventType,
        T data,
        CancellationToken cancellationToken = default)
    {
        var message = new SseMessage(eventType, JsonSerializer.Serialize(data, JsonOptions));
        
        // Publish to Redis for other instances
        await _subscriber.PublishAsync(
            RedisChannel.Literal($"{TenantChannel}{tenantId}"),
            JsonSerializer.Serialize(message, JsonOptions));

        // Also send to local connections
        await SendToLocalTenantAsync(tenantId, eventType, data, cancellationToken);
    }

    public async Task BroadcastAsync<T>(
        string eventType,
        T data,
        CancellationToken cancellationToken = default)
    {
        var message = new SseMessage(eventType, JsonSerializer.Serialize(data, JsonOptions));
        
        // Publish to Redis for other instances
        await _subscriber.PublishAsync(
            RedisChannel.Literal(BroadcastChannel),
            JsonSerializer.Serialize(message, JsonOptions));

        // Also send to local connections
        await SendToAllLocalAsync(eventType, data, cancellationToken);
    }

    public async Task SendToConnectionsAsync<T>(
        IEnumerable<string> connectionIds,
        string eventType,
        T data,
        CancellationToken cancellationToken = default)
    {
        var connections = _connectionManager.GetAllConnections()
            .Where(c => connectionIds.Contains(c.ConnectionId));

        var tasks = connections.Select(c => 
            c.WriteEventAsync(eventType, data, cancellationToken: cancellationToken));

        await Task.WhenAll(tasks);
    }

    private async Task SendToLocalUserAsync<T>(
        Guid userId,
        string eventType,
        T data,
        CancellationToken cancellationToken)
    {
        var connections = _connectionManager.GetConnectionsByUser(userId);
        var tasks = connections.Select(c => 
            c.WriteEventAsync(eventType, data, cancellationToken: cancellationToken));
        await Task.WhenAll(tasks);
    }

    private async Task SendToLocalTenantAsync<T>(
        Guid tenantId,
        string eventType,
        T data,
        CancellationToken cancellationToken)
    {
        var connections = _connectionManager.GetConnectionsByTenant(tenantId);
        var tasks = connections.Select(c => 
            c.WriteEventAsync(eventType, data, cancellationToken: cancellationToken));
        await Task.WhenAll(tasks);
    }

    private async Task SendToAllLocalAsync<T>(
        string eventType,
        T data,
        CancellationToken cancellationToken)
    {
        var connections = _connectionManager.GetAllConnections();
        var tasks = connections.Select(c => 
            c.WriteEventAsync(eventType, data, cancellationToken: cancellationToken));
        await Task.WhenAll(tasks);
    }

    private async Task HandleUserMessage(string channel, string message)
    {
        try
        {
            var userIdStr = channel.Replace(UserChannel, "");
            if (!Guid.TryParse(userIdStr, out var userId))
                return;

            var sseMessage = JsonSerializer.Deserialize<SseMessage>(message, JsonOptions);
            if (sseMessage is null)
                return;

            var connections = _connectionManager.GetConnectionsByUser(userId);
            var tasks = connections.Select(c => 
                c.WriteEventAsync(sseMessage.EventType, sseMessage.Data));
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error handling user SSE message");
        }
    }

    private async Task HandleTenantMessage(string channel, string message)
    {
        try
        {
            var tenantIdStr = channel.Replace(TenantChannel, "");
            if (!Guid.TryParse(tenantIdStr, out var tenantId))
                return;

            var sseMessage = JsonSerializer.Deserialize<SseMessage>(message, JsonOptions);
            if (sseMessage is null)
                return;

            var connections = _connectionManager.GetConnectionsByTenant(tenantId);
            var tasks = connections.Select(c => 
                c.WriteEventAsync(sseMessage.EventType, sseMessage.Data));
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error handling tenant SSE message");
        }
    }

    private async Task HandleBroadcastMessage(string message)
    {
        try
        {
            var sseMessage = JsonSerializer.Deserialize<SseMessage>(message, JsonOptions);
            if (sseMessage is null)
                return;

            var connections = _connectionManager.GetAllConnections();
            var tasks = connections.Select(c => 
                c.WriteEventAsync(sseMessage.EventType, sseMessage.Data));
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error handling broadcast SSE message");
        }
    }

    public void Dispose()
    {
        _subscriber.UnsubscribeAll();
    }

    private sealed record SseMessage(string EventType, string Data);
}
