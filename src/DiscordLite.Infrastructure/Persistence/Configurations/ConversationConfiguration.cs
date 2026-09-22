using DiscordLite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DiscordLite.Infrastructure.Persistence.Configurations;

public sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.ConversationType).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        
        builder.Property(x => x.Name)
            .HasMaxLength(100);

        builder.Property(x => x.AvatarUrl)
            .HasMaxLength(2048);
        
        builder.HasMany(x => x.Participants)
            .WithOne()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.DirectUser1Id, x.DirectUser2Id })
            .IsUnique()
            .HasDatabaseName("Conversation_DirectUsers");
    }
}