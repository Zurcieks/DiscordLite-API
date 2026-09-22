using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace DiscordLite.Infrastructure.Storage;

public sealed class MinioBucketInitializer(
    IMinioClient client,
    IOptions<MinioOptions> options,
    ILogger<MinioBucketInitializer> logger) : IHostedService

{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var bucketName = options.Value.Bucket; 
        
        var existsArgs = new BucketExistsArgs()
            .WithBucket(bucketName);
        
        var exists = await client.BucketExistsAsync(existsArgs, cancellationToken);

        if (!exists)
        {
            var createArgs = new MakeBucketArgs().WithBucket(bucketName);
            await client.MakeBucketAsync(createArgs, cancellationToken);
            logger.LogInformation("Bucket created with name {BucketName}", bucketName);
        }

        // Publiczny jest tylko odczyt obiektów, bez zapisu i listowania bucketa.
        var policy = $$"""
        {
          "Version": "2012-10-17",
          "Statement": [
            {
              "Effect": "Allow",
              "Principal": "*",
              "Action": "s3:GetObject",
              "Resource": "arn:aws:s3:::{{bucketName}}/*"
            }
          ]
        }
        """;

        await client.SetPolicyAsync(
            new SetPolicyArgs().WithBucket(bucketName).WithPolicy(policy),
            cancellationToken);

        logger.LogInformation("Public object read enabled for bucket {BucketName}", bucketName);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
