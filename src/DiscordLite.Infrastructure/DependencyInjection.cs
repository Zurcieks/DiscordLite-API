using System.Runtime.InteropServices.ComTypes;
using DiscordLite.Application.Abstractions;
using DiscordLite.Infrastructure.BackgroundServices;
using DiscordLite.Infrastructure.Persistence;
using DiscordLite.Infrastructure.Persistence.Repositories;
using DiscordLite.Infrastructure.Security;
using DiscordLite.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Minio;

namespace DiscordLite.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
                                ?? throw new InvalidOperationException("Connection string was not found");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.Configure<MinioOptions>(
            configuration.GetSection("Minio")); 
        // laczy json z MiniopOptions, zeby inne uslugi mogly pobrac ustawienia przez ioptions

        services.AddSingleton<IMinioClient>(provider =>
        {
            var options = provider
                .GetRequiredService<IOptions<MinioOptions>>()
                .Value;

            return new MinioClient()
                .WithEndpoint(options.Endpoint)
                .WithCredentials(options.AccessKey, options.SecretKey)
                .WithSSL(options.UseSsl)
                .Build();
        });
        
        services.AddScoped<IAvatarContentValidator, AvatarContentValidator>();
        services.AddScoped<IAvatarStorage, AvatarStorage>();
        services.AddHostedService<MinioBucketInitializer>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddHostedService<RefreshTokenCleanupService>();
        services.AddScoped<IFriendshipRepository, FriendshipRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        return services;
    }
}