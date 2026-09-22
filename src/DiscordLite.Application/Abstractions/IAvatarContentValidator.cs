namespace DiscordLite.Application.Abstractions;

public interface IAvatarContentValidator
{
    Task ValidateAsync(
        Stream content,
        string declaredContentType,
        CancellationToken ct);
}