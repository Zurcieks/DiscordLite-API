using DiscordLite.Application.Abstractions;
using DiscordLite.Application.Exceptions;
using DiscordLite.Domain.Entities;
using MediatR;

namespace DiscordLite.Application.Conversations.StartDirectConversation;

public class StartDirectConversationCommandHandler(
    ICurrentUser currentUser,
    IUserRepository userRepository,
    IFriendshipRepository friendshipRepository,
    IConversationRepository conversationRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<StartDirectConversationCommand, StartDirectConversationResponse>
{
    public async Task<StartDirectConversationResponse> Handle(StartDirectConversationCommand request,
        CancellationToken ct)
    {
        var userId = currentUser.UserId;

        var targetUser = await userRepository.GetByIdAsync(request.TargetUserId, ct);
        if (targetUser is null)
            throw new NotFoundException("USER_NOT_FOUND", "User not found");

        var relation = await friendshipRepository.GetBetweenAsync(userId, targetUser.Id, ct);

        if (relation is null || relation.Status != FriendshipStatus.Accepted)
            throw new ForbiddenException("CONVERSATION_USERS_NOT_FRIENDS", "You are not friends");

        var existingConversation = await conversationRepository.GetDirectBetweenAsync(userId, targetUser.Id, ct);
        if (existingConversation is not null)
        {
            return new StartDirectConversationResponse(existingConversation.Id);
        }

        var conversation = Conversation.CreateDirect(userId, targetUser.Id);
        await conversationRepository.AddAsync(conversation, ct);
        try
        {
            await unitOfWork.SaveChangesAsync(ct);
            return new StartDirectConversationResponse(conversation.Id);
        }
        catch (DirectConversationAlreadyExistsException)
        {
            var existing = await conversationRepository
                .GetDirectBetweenAsync(userId, targetUser.Id, ct);

            if (existing is null)
                throw new ConflictException("CONVERSATION_CONFLICT", "Conversation conflict could not be resolved");

            return new StartDirectConversationResponse(existing.Id);
        }
    }

}