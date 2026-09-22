using MediatR;

namespace DiscordLite.Application.Users.UploadAvatar;

public sealed record UploadAvatarCommand(
    Stream Content,
    long ContentLength,
    string ContentType) : IRequest<UploadAvatarResponse>;

public sealed record UploadAvatarResponse(string? AvatarUrl);
