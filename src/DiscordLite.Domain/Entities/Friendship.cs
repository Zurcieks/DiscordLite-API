using DiscordLite.Domain.Exceptions;

namespace DiscordLite.Domain.Entities
{
    public sealed class Friendship
    {
        public Guid Id { get; private set; }
        public Guid SenderId { get; private set; }
        public Guid ReceiverId { get; private set; }
        public FriendshipStatus Status { get; private set; }
        public DateTime? RespondedAt { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private Friendship() { }

        public static Friendship Create(Guid senderId, Guid receiverId)
        {
            if (senderId == Guid.Empty)
                throw new DomainValidationException(
                    "FRIENDSHIP_SENDER_ID_EMPTY",
                    "SenderId cannot be empty.");

            if (receiverId == Guid.Empty)
                throw new DomainValidationException(
                    "FRIENDSHIP_RECEIVER_ID_EMPTY",
                    "ReceiverId cannot be empty.");

            if (senderId == receiverId)
                throw new DomainValidationException(
                    "FRIENDSHIP_SELF_REQUEST",
                    "You cannot send a friend request to yourself.");

            return new Friendship
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                ReceiverId = receiverId,
                Status = FriendshipStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void Accept(Guid receiverId)
        {
            if (receiverId == Guid.Empty)
                throw new DomainValidationException(
                    "FRIENDSHIP_RECEIVER_ID_EMPTY",
                    "ReceiverId cannot be empty.");

            if (receiverId != ReceiverId)
                throw new DomainForbiddenException(
                    "FRIENDSHIP_NOT_REQUEST_RECEIVER",
                    "Only the receiver can accept the friend request.");

            if (Status != FriendshipStatus.Pending)
                throw new DomainConflictException(
                    "FRIENDSHIP_REQUEST_NOT_PENDING",
                    "Only pending friend requests can be accepted.");

            Status = FriendshipStatus.Accepted;
            RespondedAt = DateTime.UtcNow;
        }
    }

    public enum FriendshipStatus
    {
        Pending,
        Accepted
    }
}