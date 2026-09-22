using DiscordLite.Application.Abstractions;
using MediatR;

namespace DiscordLite.Application.Conversations.GetConversations;

public class GetConversationsQueryHandler(
    ICurrentUser currentUser,
    IConversationRepository conversationRepository) : IRequestHandler<GetConversationsQuery, GetConversationsResponse>
{
    public async Task<GetConversationsResponse> Handle(GetConversationsQuery request, CancellationToken ct)
    {
        var userId = currentUser.UserId;

        var conversations = await conversationRepository.GetForUserAsync(userId, ct);
        
        return new GetConversationsResponse(conversations);
    }
}