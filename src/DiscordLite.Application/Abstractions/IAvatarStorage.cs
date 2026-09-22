namespace DiscordLite.Application.Abstractions;

public interface IAvatarStorage
{
    string? GetPublicUrl(string? avatarKey);

    Task<string> UploadAsync(
        Stream content,
        long contentLength,
        string contentType,
        CancellationToken cancellationToken);
}
