using MediatR;

namespace DiscordLite.Application.Friendships.AddFriend
{
    public sealed record AddFriendCommand(string Username) : IRequest<AddFriendResponse>;

    public sealed record AddFriendResponse(string Message);
}
