using AlfaCRM.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlfaCRM.Infrastructure.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<MessageEntity>
{
    public void Configure(EntityTypeBuilder<MessageEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("uuid");
        builder.HasIndex(x => x.Id).IsUnique();
        
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IsDeleted).IsRequired();
        builder.Property(x => x.IsPinned).IsRequired();
        builder.Property(x => x.SenderId).IsRequired();
        builder.Property(x => x.ChatId).IsRequired();

        builder.HasOne(x => x.Sender)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.SenderId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Chat)
            .WithMany(x => x.Messages)
            .HasForeignKey(x => x.ChatId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.RepliedMessage)
            .WithMany(x => x.Replies)
            .HasForeignKey(x => x.RepliedMessageId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(x => x.Replies)
            .WithOne(x => x.RepliedMessage)
            .HasForeignKey(x => x.RepliedMessageId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}