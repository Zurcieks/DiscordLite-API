using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace DiscordLite.Api.Presence;

public class SubUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        var subject = connection.User.FindFirstValue("sub");
        
        return Guid.TryParse(subject, out var userId)
            ? userId.ToString()
            : null;
    }
}