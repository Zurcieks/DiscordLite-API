namespace DiscordLite.Api.Presence;

public sealed class PresenceTracker(ILogger<PresenceTracker> logger)
{
    private readonly object _lock = new();

    private readonly Dictionary<Guid, HashSet<string>> _connections = new();

    public bool Connect(Guid userId, string connectionId)
    {
        lock (_lock) // zapewnia ochrone przed race condition
        {
            if (!_connections.TryGetValue(userId, out var connections))
            {
                connections = new HashSet<string>();
                _connections.Add(userId, connections);
            }
            
            var added = connections.Add(connectionId);
            
            logger.LogInformation(
                "Connected: UserId={UserId}, ConnectionId={ConnectionId}, Count={Count}",
                userId,
                connectionId,
                connections.Count);

            return added && connections.Count == 1;
        }
    }

    public bool Disconnect(Guid userId, string connectionId)
    {
        lock (_lock)
        {
            if (!_connections.TryGetValue(userId, out var connections))
                return false;

            if (!connections.Remove(connectionId))
                return false;

            var becameOffline = connections.Count == 0;
            
            if(becameOffline)
                _connections.Remove(userId);
            
            logger.LogInformation(
                "Disconnected: UserId={UserId}, ConnectionId={ConnectionId}, Count={Count}",
                userId,
                connectionId,
                connections.Count);

            return becameOffline;
        }
    }

    public bool IsOnline(Guid userId)
    {
        lock (_lock)
        {
            return _connections.ContainsKey(userId);
        }
    }
}