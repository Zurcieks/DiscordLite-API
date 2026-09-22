using DiscordLite.Application.Abstractions;
using DiscordLite.Application.Exceptions;
using DiscordLite.Domain.Entities;
using MediatR;

namespace DiscordLite.Application.Auth.Register;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IRefreshTokenCookieWriter cookieWriter,
    IPasswordService passwordService,
    ITokenService tokenService,
    IUnitOfWork unitOfWork,
    IAvatarStorage avatarStorage)
    : IRequestHandler<RegisterUserCommand, RegisterUserResponse>
{
    public async Task<RegisterUserResponse> Handle(
        RegisterUserCommand request,
        CancellationToken ct)
    {
        var normalized = User.NormalizeUsername(request.Username);

        var exists = await userRepository
            .ExistsByNormalizedUsernameAsync(normalized, ct);

        if (exists)
        {
            throw new ConflictException(
                "AUTH_USERNAME_ALREADY_EXISTS",
                "User with this username already exists.");
        }

        var passwordHash = passwordService.Hash(request.Password);

        var user = User.Create(
            request.Username,
            passwordHash);

        await userRepository.AddAsync(user, ct);

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
        

        return new RegisterUserResponse(
            user.Id,
            user.Username,
            accessToken);
    }
}
