using DiscordLite.Domain.Exceptions;

namespace DiscordLite.Domain.Entities;

public sealed class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = null!;
    public string NormalizedUsername { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string? AvatarKey { get; private set; }

    private User() { }

    public static User Create(string username, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new DomainValidationException(
                "USER_USERNAME_EMPTY",
                "Username cannot be empty.");

        var trimmedUsername = username.Trim();

        if (trimmedUsername.Length is < 3 or > 30)
            throw new DomainValidationException(
                "USER_USERNAME_INVALID_LENGTH",
                "Username must be between 3 and 30 characters long.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainValidationException(
                "USER_PASSWORD_HASH_EMPTY",
                "Password hash cannot be null or empty.");

        return new User
        {
            Id = Guid.NewGuid(),
            Username = trimmedUsername,
            NormalizedUsername = trimmedUsername.ToLowerInvariant(),
            PasswordHash = passwordHash
        };
    }

    public void ChangeAvatar(string? avatarKey)
    {
        if (avatarKey is not null && string.IsNullOrWhiteSpace(avatarKey))
            throw new DomainValidationException(
                "USER_AVATAR_URL_INVALID",
                "Avatar URL cannot be empty or whitespace.");

        AvatarKey = avatarKey;
    }

    public static string NormalizeUsername(string username)
    {
        return username.Trim().ToLowerInvariant();
    }
}