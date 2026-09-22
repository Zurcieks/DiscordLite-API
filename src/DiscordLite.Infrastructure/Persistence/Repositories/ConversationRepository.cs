using DiscordLite.Application.Abstractions;
using DiscordLite.Application.Conversations.GetConversations;
using DiscordLite.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DiscordLite.Infrastructure.Persistence.Repositories;

public class ConversationRepository(AppDbContext context, IAvatarStorage avatarStorage) : RepositoryBase<Conversation>(context), IConversationRepository 
{
    public async Task<Conversation?> GetDirectBetweenAsync(Guid userId1, Guid userId2, CancellationToken ct)
    {
        Guid directUser1Id;
        Guid directUser2Id;

        if (userId1.CompareTo(userId2) < 0)
        {
            directUser1Id = userId1;
            directUser2Id = userId2;
            
        }
        else
        {
            directUser1Id = userId2;
            directUser2Id = userId1;
        }

        return await Context.Conversations
            .FirstOrDefaultAsync(
                x =>
                    x.ConversationType == ConversationType.Direct &&
                    x.DirectUser1Id == directUser1Id &&
                    x.DirectUser2Id == directUser2Id, ct);
    }

    public async Task<List<ConversationDto>> GetForUserAsync(Guid userId, CancellationToken ct)
    {
        var conversations =
            from currentParticipant in Context.ConversationParticipants
            where currentParticipant.UserId == userId
            join conversation in Context.Conversations
                on currentParticipant.ConversationId equals conversation.Id
            join otherParticipant in Context.ConversationParticipants
                on conversation.Id equals otherParticipant.ConversationId
            where otherParticipant.UserId != userId
                  && conversation.ConversationType == ConversationType.Direct

            join otherUser in Context.Users
                on otherParticipant.UserId equals otherUser.Id

            select new
            {
                conversation.Id,
                conversation.ConversationType,
                otherUser.Username,
                otherUser.AvatarKey
            };
        
        var rows = await conversations.ToListAsync(ct);

        return rows.Select(row => new ConversationDto(
            row.Id,
            row.ConversationType,
            row.Username,
            avatarStorage.GetPublicUrl(row.AvatarKey))).ToList();

    }
}