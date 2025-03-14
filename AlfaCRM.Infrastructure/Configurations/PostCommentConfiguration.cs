using AlfaCRM.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlfaCRM.Infrastructure.Configurations;

public class PostCommentConfiguration : IEntityTypeConfiguration<PostCommentEntity>
{
    public void Configure(EntityTypeBuilder<PostCommentEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("uuid");
        builder.HasIndex(x => x.Id).IsUnique();
            
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IsDeleted).IsRequired();
        builder.Property(x => x.PostId).IsRequired();
        builder.Property(x => x.SenderId).IsRequired();
            
        builder.HasOne(x => x.Post)
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Sender)
            .WithMany(x => x.Comments)
            .HasForeignKey(x => x.SenderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}