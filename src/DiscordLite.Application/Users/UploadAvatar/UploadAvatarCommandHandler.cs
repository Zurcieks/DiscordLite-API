using DiscordLite.Application.Abstractions;
using DiscordLite.Application.Exceptions;
using MediatR;

namespace DiscordLite.Application.Users.UploadAvatar;

public sealed class UploadAvatarCommandHandler(
    ICurrentUser currentUser,
    IUserRepository userRepository,
    IAvatarStorage avatarStorage,
    IAvatarContentValidator avatarContentValidator,
    IUnitOfWork unitOfWork): IRequestHandler<UploadAvatarCommand, UploadAvatarResponse>
{
    public async Task<UploadAvatarResponse> Handle(UploadAvatarCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetByIdAsync(currentUser.UserId, ct);

        if (user is null)
        {
            throw new NotFoundException("USER_NOT_FOUND", "User not found.");
        }

        await avatarContentValidator.ValidateAsync(request.Content, request.ContentType, ct);

        var avatarKey =
            await avatarStorage.UploadAsync(request.Content, request.ContentLength, request.ContentType, ct);
        
        user.ChangeAvatar(avatarKey);

        await unitOfWork.SaveChangesAsync(ct);
        
        return new UploadAvatarResponse(avatarStorage.GetPublicUrl(avatarKey));
    }
}
