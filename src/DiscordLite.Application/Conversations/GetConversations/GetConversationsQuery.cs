using DiscordLite.Domain.Entities;
using MediatR;

namespace DiscordLite.Application.Conversations.GetConversations;

public sealed record GetConversationsQuery() : IRequest<GetConversationsResponse>;

public sealed record GetConversationsResponse(List<ConversationDto> Conversations);

public sealed record ConversationDto(
    Guid ConversationId,
    ConversationType Type,
    string DisplayName,
    string? AvatarUrl);
    
