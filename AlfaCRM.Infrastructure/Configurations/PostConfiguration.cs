using AlfaCRM.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlfaCRM.Infrastructure.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<PostEntity>
{
    public void Configure(EntityTypeBuilder<PostEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("uuid");
        builder.HasIndex(x => x.Id).IsUnique();
            
        builder.Property(x => x.Title).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Subtitle).HasMaxLength(50);
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IsImportant).IsRequired();
        builder.Property(x => x.IsActual).IsRequired();
        builder.Property(x => x.PublisherId).IsRequired();
            
        builder.HasOne(x => x.Department)
            .WithMany(x => x.Posts)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Publisher)
            .WithMany(x => x.Posts)
            .HasForeignKey(x => x.PublisherId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Reactions)
            .WithOne(x => x.Post)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Comments)
            .WithOne(x => x.Post)
            .HasForeignKey(x => x.PostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}