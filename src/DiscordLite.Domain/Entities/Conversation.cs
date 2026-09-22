using DiscordLite.Domain.Exceptions;

namespace DiscordLite.Domain.Entities;

public sealed class Conversation
{
    public Guid Id { get; private set; }
    public Guid? DirectUser1Id { get; private set; }
    public Guid? DirectUser2Id { get; private set; }
    public string? Name { get; private set; }
    public string? AvatarUrl { get; private set; }
    public ConversationType ConversationType { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    private readonly List<ConversationParticipant> _participants = new();
    public IReadOnlyCollection<ConversationParticipant> Participants => _participants;
    
    
    
    private Conversation() {}

    public static Conversation CreateDirect(Guid userId1, Guid userId2)
    {
        if(userId1 == Guid.Empty)
            throw new DomainValidationException("CONVERSATION_USER_ID_EMPTY","User id cannot be empty.");
        
        if(userId2 == Guid.Empty)
            throw new DomainValidationException("CONVERSATION_USER_ID_EMPTY","User id cannot be empty.");
        if (userId1 == userId2)
            throw new DomainValidationException("CONVERSATION_SELF_REQUEST", "You cannot create a conversation with yourself");

        Guid directUser1Id;
        Guid directUser2Id;
        if(userId1.CompareTo(userId2) < 0)
        {
            directUser1Id = userId1;
            directUser2Id = userId2;
        }
        else
        {
            directUser1Id = userId2;
            directUser2Id = userId1;
        }

        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            DirectUser1Id =  directUser1Id,
            DirectUser2Id =  directUser2Id,
            ConversationType = ConversationType.Direct,
            CreatedAt = DateTime.UtcNow,
        };

        var participant1 = ConversationParticipant.Create(
            conversation.Id, userId1);
        var participant2 = ConversationParticipant.Create(
            conversation.Id, userId2);
        conversation._participants.Add(participant1);
        conversation._participants.Add(participant2);
        
        return conversation;
    }

   
}

public enum ConversationType  {
    Direct,
    Group
}