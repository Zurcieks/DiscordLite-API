using DiscordLite.Application.Abstractions;
using DiscordLite.Application.Friendships.GetAllFriends;
using DiscordLite.Application.Friendships.GetFriendsRequest;
using DiscordLite.Domain.Entities;
using DiscordLite.Infrastructure.Persistence;
using DiscordLite.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordLite.Infrastructure.Persistence.Repositories
{
    public sealed class FriendshipRepository(AppDbContext context, IAvatarStorage avatarStorage) : RepositoryBase<Friendship>(context), IFriendshipRepository
    {
        public async Task<List<FriendshipDto>> GetAllFriends(Guid userId, CancellationToken ct)
        {
            var rows = await Context.Friendships
                .Where(x => x.Status == FriendshipStatus.Accepted && 
                (x.SenderId == userId || x.ReceiverId == userId)) // jedna z stron relacji
                .Join(
                Context.Users,
                friends => friends.SenderId == userId ? friends.ReceiverId : friends.SenderId, 
                user => user.Id,
                (friends, user) => new
                {
                    FriendshipId = friends.Id,
                    UserId = user.Id,
                    user.Username,
                    user.AvatarKey
                }).ToListAsync(ct);

            return rows.Select(row => new FriendshipDto(
                row.FriendshipId,
                row.UserId,
                row.Username,
                avatarStorage.GetPublicUrl(row.AvatarKey))).ToList();
                   
        }


        public async Task<Friendship?> GetBetweenAsync(Guid userId1, Guid userId2, CancellationToken ct)
        {
            return await Context.Friendships
                .FirstOrDefaultAsync(x => (x.SenderId == userId1 && x.ReceiverId == userId2) || (x.SenderId == userId2 && x.ReceiverId == userId1), ct);
        }

        public async Task<Friendship?> GetByIdAsync(Guid friendshipId, CancellationToken ct)
        {
            return await Context.Friendships
                .FirstOrDefaultAsync(x => x.Id == friendshipId, ct);
        }

        public async Task<List<FriendRequestDto>> GetIncomingAndOutgoingRequests(Guid userId, CancellationToken ct)
        {
            var rows = await Context.Friendships
                .Where(x => x.Status == FriendshipStatus.Pending && (x.SenderId == userId || x.ReceiverId == userId))
                .Join(
                Context.Users,
                friends => friends.SenderId == userId ? friends.ReceiverId : friends.SenderId,
                user => user.Id,
                (friends, user) => new
                {
                    FriendshipId = friends.Id,
                    UserId = user.Id,
                    user.Username,
                    user.AvatarKey,
                    friends.CreatedAt,
                    IsIncoming = friends.ReceiverId == userId
                }).ToListAsync(ct);

            return rows.Select(row => new FriendRequestDto(
                row.FriendshipId,
                row.UserId,
                row.Username,
                avatarStorage.GetPublicUrl(row.AvatarKey),
                row.CreatedAt,
                row.IsIncoming)).ToList();
                   
        }
    }
}
