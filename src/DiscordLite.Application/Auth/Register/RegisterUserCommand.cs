using MediatR;

namespace DiscordLite.Application.Auth.Register;

public sealed record RegisterUserCommand(string Username, string Password) : IRequest<RegisterUserResponse>;
public sealed record RegisterUserResponse(Guid UserId, string Username, string AccessToken);