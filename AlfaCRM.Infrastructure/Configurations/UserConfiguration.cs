using AlfaCRM.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlfaCRM.Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("uuid");
        builder.HasIndex(x => x.Id).IsUnique();
            
        builder.Property(x => x.Username).HasMaxLength(50).IsRequired();
        builder.Property(x => x.FullName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(50).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(300).IsRequired();
        builder.Property(x => x.HiredAt).IsRequired();
        builder.Property(x => x.IsMale).IsRequired();
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.IsAdmin).IsRequired();
        builder.Property(x => x.HasPublishedRights).IsRequired();
        builder.Property(x => x.IsBlocked).IsRequired();
            
        builder.HasIndex(x => x.Username).IsUnique();
        builder.HasIndex(x => x.Email).IsUnique();

        builder.HasOne(x => x.Department)
            .WithMany(x => x.Users)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasMany(x => x.Posts)
            .WithOne(x => x.Publisher)
            .HasForeignKey(x => x.PublisherId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Comments)
            .WithOne(x => x.Sender)
            .HasForeignKey(x => x.SenderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Reactions)
            .WithOne(x => x.Sender)
            .HasForeignKey(x => x.SenderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}