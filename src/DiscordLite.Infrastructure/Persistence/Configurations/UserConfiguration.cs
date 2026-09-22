using DiscordLite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DiscordLite.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedNever();

        builder.Property(u => u.Username)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(u => u.NormalizedUsername)
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(u => u.AvatarKey)
            .HasMaxLength(2048);
        
        builder.HasIndex(u => u.NormalizedUsername)
            .IsUnique();
        
       

    }
}