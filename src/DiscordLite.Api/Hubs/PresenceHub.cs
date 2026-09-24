using DiscordLite.Api.Presence;
using DiscordLite.Application.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace DiscordLite.Api.Hubs;

public sealed class PresenceHub(
    PresenceTracker tracker,
    IFriendshipRepository friendshipRepository,
    ILogger<PresenceHub> logger) : Hub
{
    public override async Task OnConnectedAsync()
    {
        if (!Guid.TryParse(Context.UserIdentifier, out var userId))
        {
            Context.Abort();
            return;
        }

        var becameOnline = tracker.Connect(
            userId,
            Context.ConnectionId);

        if (becameOnline)
        {
            await NotifyFriendsAsync(userId, "UserOnline");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (Guid.TryParse(Context.UserIdentifier, out var userId))
        {
            var becameOffline = tracker.Disconnect(userId, Context.ConnectionId);

            if (becameOffline)
            {
                await NotifyFriendsAsync(userId, "UserOffline");
            }
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task<Guid[]> GetOnlineFriends()
    {
        if (!Guid.TryParse(Context.UserIdentifier, out var userId))
            throw new HubException("Invalid user identifier");

        var friends = await friendshipRepository.GetAllFriends(userId, Context.ConnectionAborted);

        return friends
            .Select(friend => friend.UserId)
            .Distinct()
            .Where(tracker.IsOnline)
            .ToArray();
    }

    private async Task NotifyFriendsAsync(Guid userId, string eventName)
    {
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var friends = await friendshipRepository.GetAllFriends(userId, timeout.Token);

            var friendIds = friends
                .Select(friend => friend.UserId.ToString())
                .Distinct()
                .ToArray();

            if (friendIds.Length == 0)
                return;

            await Clients.Users(friendIds).SendAsync(
                eventName,
                userId,
                timeout.Token);
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Could not send {EventName} for user {UserId}",
                eventName,
                userId);
        }
    }
}