using MediatR;

namespace DiscordLite.Application.Conversations.StartDirectConversation;

public sealed record StartDirectConversationCommand(Guid TargetUserId) : IRequest<StartDirectConversationResponse>;

public sealed record StartDirectConversationResponse(Guid ConversationId);    
