using DiscordLite.Application.Abstractions;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace DiscordLite.Infrastructure.Storage;

public sealed class AvatarStorage(
    IMinioClient client,
    IOptions<MinioOptions> options) : IAvatarStorage
{
    public string? GetPublicUrl(string? avatarKey)
    {
        if (string.IsNullOrWhiteSpace(avatarKey))
            return null;

        var baseUrl = options.Value.PublicBaseUrl.TrimEnd('/');
        var bucket = options.Value.Bucket;

        return $"{baseUrl}/{bucket}/{Uri.EscapeDataString(avatarKey)}";
    }

    public async Task<string> UploadAsync(Stream content, long contentLength, string contentType, CancellationToken cancellationToken)
    {
        var objectKey = Guid.NewGuid().ToString("N");

        var args = new PutObjectArgs()
            .WithBucket(options.Value.Bucket)
            .WithObject(objectKey)
            .WithStreamData(content)
            .WithObjectSize(contentLength) // ile bajtow wysylamy
            .WithContentType(contentType);
        
        await client.PutObjectAsync(args, cancellationToken);
        return objectKey;
    }
}
