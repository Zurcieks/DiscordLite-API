using DiscordLite.Application.Abstractions;
using DiscordLite.Application.Exceptions;
using DiscordLite.Domain.Entities;
using MediatR;
 
namespace DiscordLite.Application.Friendships.AddFriend
{
    public sealed class AddFriendCommandHandler(
        IFriendshipRepository friendshipRepository,
        IUserRepository userRepository,
        ICurrentUser currentUser,
        IUnitOfWork unitOfWork)
        : IRequestHandler<AddFriendCommand, AddFriendResponse>
    {
        public async Task<AddFriendResponse> Handle(
            AddFriendCommand request,
            CancellationToken ct)
        {
            var sender = currentUser.UserId;

            var normalized = User.NormalizeUsername(request.Username);

            var receiver = await userRepository
                .GetByNormalizedUsernameAsync(normalized, ct);

            if (receiver is null)
            {
                throw new NotFoundException(
                    "FRIENDSHIP_USER_NOT_FOUND",
                    "User not found.");
            }

            var exists = await friendshipRepository
                .GetBetweenAsync(sender, receiver.Id, ct);

            if (exists is { Status: FriendshipStatus.Pending } &&
                sender == exists.SenderId)
            {
                throw new ConflictException(
                    "FRIENDSHIP_REQUEST_ALREADY_SENT",
                    "Friend request already sent.");
            }

            if (exists is { Status: FriendshipStatus.Pending } &&
                sender == exists.ReceiverId)
            {
                throw new ConflictException(
                    "FRIENDSHIP_PENDING_REQUEST_EXISTS",
                    "You have a pending friend request from this user.");
            }

            if (exists is { Status: FriendshipStatus.Accepted })
            {
                throw new ConflictException(
                    "FRIENDSHIP_ALREADY_EXISTS",
                    "You are already friends with this user.");
            }

            var friendship = Friendship.Create(
                sender,
                receiver.Id);

            await friendshipRepository.AddAsync(friendship, ct);
            await unitOfWork.SaveChangesAsync(ct);

            return new AddFriendResponse("Friend request sent successfully.");
        }
    }
}