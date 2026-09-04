using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace SyntheticSocialWorld.Api.Services;

/// <summary>
/// Manages WebSocket connections for real-time updates
/// </summary>
public class WebSocketService
{
    private readonly ConcurrentDictionary<string, WebSocketConnection> _connections = new();
    private readonly ILogger<WebSocketService> _logger;
    
    public WebSocketService(ILogger<WebSocketService> logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// Register a new WebSocket connection
    /// </summary>
    public async Task<string> AddConnectionAsync(WebSocket socket, string playerId)
    {
        var connectionId = Guid.NewGuid().ToString();
        var connection = new WebSocketConnection
        {
            ConnectionId = connectionId,
            Socket = socket,
            PlayerId = playerId,
            ConnectedAt = DateTimeOffset.UtcNow,
            LastPing = DateTimeOffset.UtcNow
        };
        
        _connections[connectionId] = connection;
        _logger.LogInformation("WebSocket connected: {ConnectionId} for player {PlayerId}", connectionId, playerId);
        
        await SendMessageAsync(connectionId, new WebSocketMessage
        {
            Type = "connected",
            Data = new { connectionId, playerId }
        });
        
        return connectionId;
    }
    
    /// <summary>
    /// Remove a connection
    /// </summary>
    public async Task RemoveConnectionAsync(string connectionId)
    {
        if (_connections.TryRemove(connectionId, out var connection))
        {
            _logger.LogInformation("WebSocket disconnected: {ConnectionId}", connectionId);
            
            if (connection.Socket.State == WebSocketState.Open)
            {
                await connection.Socket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "Connection closed",
                    CancellationToken.None);
            }
        }
    }
    
    /// <summary>
    /// Send a message to a specific connection
    /// </summary>
    public async Task SendMessageAsync(string connectionId, WebSocketMessage message)
    {
        if (!_connections.TryGetValue(connectionId, out var connection))
            return;
        
        if (connection.Socket.State != WebSocketState.Open)
            return;
        
        try
        {
            var json = JsonSerializer.Serialize(message);
            var bytes = Encoding.UTF8.GetBytes(json);
            
            await connection.Socket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send message to {ConnectionId}", connectionId);
        }
    }
    
    /// <summary>
    /// Broadcast a message to all connections for a player
    /// </summary>
    public async Task BroadcastToPlayerAsync(string playerId, WebSocketMessage message)
    {
        var connections = _connections.Values.Where(c => c.PlayerId == playerId);
        
        foreach (var connection in connections)
        {
            await SendMessageAsync(connection.ConnectionId, message);
        }
    }
    
    /// <summary>
    /// Broadcast a message to all connections
    /// </summary>
    public async Task BroadcastAsync(WebSocketMessage message)
    {
        foreach (var connection in _connections.Values)
        {
            await SendMessageAsync(connection.ConnectionId, message);
        }
    }
    
    /// <summary>
    /// Send feed update notification
    /// </summary>
    public async Task SendFeedUpdateAsync(string playerId, FeedUpdate update)
    {
        await BroadcastToPlayerAsync(playerId, new WebSocketMessage
        {
            Type = "feed_update",
            Data = update
        });
    }
    
    /// <summary>
    /// Send relationship change notification
    /// </summary>
    public async Task SendRelationshipUpdateAsync(string playerId, RelationshipUpdate update)
    {
        await BroadcastToPlayerAsync(playerId, new WebSocketMessage
        {
            Type = "relationship_update",
            Data = update
        });
    }
    
    /// <summary>
    /// Send new post notification
    /// </summary>
    public async Task SendNewPostAsync(string playerId, NewPostNotification notification)
    {
        await BroadcastToPlayerAsync(playerId, new WebSocketMessage
        {
            Type = "new_post",
            Data = notification
        });
    }
    
    /// <summary>
    /// Send comment notification
    /// </summary>
    public async Task SendCommentNotificationAsync(string playerId, CommentNotification notification)
    {
        await BroadcastToPlayerAsync(playerId, new WebSocketMessage
        {
            Type = "new_comment",
            Data = notification
        });
    }
    
    /// <summary>
    /// Send event notification
    /// </summary>
    public async Task SendEventNotificationAsync(string playerId, EventNotification notification)
    {
        await BroadcastToPlayerAsync(playerId, new WebSocketMessage
        {
            Type = "event_notification",
            Data = notification
        });
    }
    
    /// <summary>
    /// Handle incoming message
    /// </summary>
    public async Task<WebSocketMessage?> HandleMessageAsync(string connectionId, string message)
    {
        try
        {
            var wsMessage = JsonSerializer.Deserialize<WebSocketMessage>(message);
            
            if (wsMessage == null) return null;
            
            // Update last ping
            if (_connections.TryGetValue(connectionId, out var connection))
            {
                connection.LastPing = DateTimeOffset.UtcNow;
            }
            
            // Handle different message types
            switch (wsMessage.Type.ToLowerInvariant())
            {
                case "ping":
                    return new WebSocketMessage { Type = "pong", Data = DateTimeOffset.UtcNow };
                    
                case "subscribe":
                    // Handle subscription to specific channels
                    return new WebSocketMessage 
                    { 
                        Type = "subscribed", 
                        Data = new { channel = wsMessage.Data } 
                    };
                    
                case "unsubscribe":
                    return new WebSocketMessage 
                    { 
                        Type = "unsubscribed", 
                        Data = new { channel = wsMessage.Data } 
                    };
                    
                default:
                    _logger.LogDebug("Unknown message type: {Type}", wsMessage.Type);
                    return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to handle WebSocket message");
            return new WebSocketMessage 
            { 
                Type = "error", 
                Data = new { message = "Invalid message format" } 
            };
        }
    }
    
    /// <summary>
    /// Clean up stale connections
    /// </summary>
    public async Task CleanupStaleConnectionsAsync()
    {
        var staleConnections = _connections.Values
            .Where(c => DateTimeOffset.UtcNow - c.LastPing > TimeSpan.FromMinutes(5))
            .ToList();
        
        foreach (var connection in staleConnections)
        {
            await RemoveConnectionAsync(connection.ConnectionId);
        }
        
        if (staleConnections.Count > 0)
        {
            _logger.LogInformation("Cleaned up {Count} stale WebSocket connections", staleConnections.Count);
        }
    }
    
    /// <summary>
    /// Get connection count
    /// </summary>
    public int ConnectionCount => _connections.Count;
}

/// <summary>
/// WebSocket connection info
/// </summary>
public class WebSocketConnection
{
    public string ConnectionId { get; set; } = "";
    public WebSocket Socket { get; set; } = null!;
    public string PlayerId { get; set; } = "";
    public DateTimeOffset ConnectedAt { get; set; }
    public DateTimeOffset LastPing { get; set; }
    public HashSet<string> Subscriptions { get; set; } = new();
}

/// <summary>
/// WebSocket message
/// </summary>
public class WebSocketMessage
{
    public string Type { get; set; } = "";
    public object? Data { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Feed update notification
/// </summary>
public class FeedUpdate
{
    public string Type { get; set; } = ""; // new_post, new_comment, like, share
    public string PostId { get; set; } = "";
    public string AuthorId { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public string ContentPreview { get; set; } = "";
    public int NewCount { get; set; }
}

/// <summary>
/// Relationship update notification
/// </summary>
public class RelationshipUpdate
{
    public string NpcId { get; set; } = "";
    public string NpcName { get; set; } = "";
    public string ChangeType { get; set; } = ""; // new_follower, unfollowed, relationship_changed
    public string? Details { get; set; }
}

/// <summary>
/// New post notification
/// </summary>
public class NewPostNotification
{
    public string PostId { get; set; } = "";
    public string AuthorId { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public string Content { get; set; } = "";
    public string? CommunityId { get; set; }
    public string? CommunityName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>
/// Comment notification
/// </summary>
public class CommentNotification
{
    public string CommentId { get; set; } = "";
    public string PostId { get; set; } = "";
    public string AuthorId { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public string Content { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>
/// Event notification
/// </summary>
public class EventNotification
{
    public string EventId { get; set; } = "";
    public string EventName { get; set; } = "";
    public string Type { get; set; } = ""; // upcoming, started, ended, reminder
    public DateTimeOffset Time { get; set; }
    public string? Details { get; set; }
}
