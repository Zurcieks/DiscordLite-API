using DiscordLite.Domain.Exceptions;

namespace DiscordLite.Domain.Entities;

public sealed class ConversationParticipant
{
    public Guid Id { get; private set; }
    public Guid ConversationId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime JoinedAt { get; private set; }
    
    private ConversationParticipant() {}

    public static ConversationParticipant Create(Guid conversationId, Guid userId)
    {
        if(userId == Guid.Empty)
            throw new DomainValidationException("CONVERSATION_USER_ID_EMPTY","User id cannot be empty.");
        if(conversationId == Guid.Empty)
            throw new DomainValidationException("CONVERSATION_ID_EMPTY", "Conversation ID cannot be empty.");
        
        return new ConversationParticipant
        {
            Id = Guid.NewGuid(),
            ConversationId =  conversationId,
            UserId = userId,
            JoinedAt = DateTime.UtcNow
        };
    }
}