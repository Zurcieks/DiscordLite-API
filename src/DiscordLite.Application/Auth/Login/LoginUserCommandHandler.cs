using DiscordLite.Application.Abstractions;
using DiscordLite.Application.Exceptions;
using DiscordLite.Domain.Entities;
using MediatR;

namespace DiscordLite.Application.Auth.Login;

public sealed class LoginUserCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordService passwordService,
    ITokenService tokenService,
    IRefreshTokenCookieWriter cookieWriter,
    IUnitOfWork unitOfWork,
    IAvatarStorage avatarStorage)
    : IRequestHandler<LoginUserCommand, LoginUserResponse>
{
    public async Task<LoginUserResponse> Handle(
        LoginUserCommand request,
        CancellationToken ct)
    {
        var normalized = User.NormalizeUsername(request.Username);

        var user = await userRepository
            .GetByNormalizedUsernameAsync(normalized, ct);

        if (user is null ||
            !passwordService.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException(
                "AUTH_INVALID_CREDENTIALS",
                "Invalid username or password.");
        }

        var accessToken = tokenService.GenerateAccessToken(
            user.Id,
            user.Username);

        var refreshTokenPlain = tokenService.GenerateRefreshToken();
        var refreshTokenHash = tokenService.HashRefreshToken(refreshTokenPlain);

        var refreshToken = RefreshToken.Create(
            user.Id,
            refreshTokenHash,
            tokenService.GetRefreshTokenExpiry());

        await refreshTokenRepository.AddAsync(refreshToken, ct);
        await unitOfWork.SaveChangesAsync(ct);

        cookieWriter.Write(refreshTokenPlain);

        return new LoginUserResponse(
            user.Id,
            user.Username,
            avatarStorage.GetPublicUrl(user.AvatarKey),
            accessToken);
    }
}
