namespace DiscordLite.Infrastructure.Storage;

public sealed class MinioOptions
{
    public string Endpoint { get; set; } = string.Empty;
    public string PublicBaseUrl { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public bool UseSsl { get; set; } = false;
    public string Bucket { get; set; } = "avatars";
}
