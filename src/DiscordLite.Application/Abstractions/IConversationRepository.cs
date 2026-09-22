using DiscordLite.Application.Conversations.GetConversations;
using DiscordLite.Domain.Entities;

namespace DiscordLite.Application.Abstractions;

public interface IConversationRepository : IRepository<Conversation>
{
    Task<Conversation?> GetDirectBetweenAsync(Guid userId1, Guid userId2, CancellationToken ct);
    Task<List<ConversationDto>> GetForUserAsync(Guid userId, CancellationToken ct);
}