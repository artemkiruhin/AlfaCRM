using AlfaCRM.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace AlfaCRM.Infrastructure;

public class AppDbContext : DbContext
{
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<PostEntity> Posts { get; set; }
    public DbSet<PostReactionEntity> PostReactions { get; set; }
    public DbSet<PostCommentEntity> PostComments { get; set; }
    public DbSet<DepartmentEntity> Departments { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<UserEntity>(opt =>
        {
            opt.HasKey(x => x.Id);
            opt.Property(x => x.Id).HasColumnType("uuid");
            opt.HasIndex(x => x.Id).IsUnique();
            
            opt.Property(x => x.Username).HasMaxLength(50).IsRequired();
            opt.Property(x => x.Email).HasMaxLength(50).IsRequired();
            opt.Property(x => x.PasswordHash).HasMaxLength(300).IsRequired();
            opt.Property(x => x.HiredAt).IsRequired();
            opt.Property(x => x.IsMale).IsRequired();
            opt.Property(x => x.IsActive).IsRequired();
            opt.Property(x => x.IsAdmin).IsRequired();
            opt.Property(x => x.HasPublishedRights).IsRequired();
            opt.Property(x => x.IsBlocked).IsRequired();
            
            opt.HasIndex(x => x.Username).IsUnique();
            opt.HasIndex(x => x.Email).IsUnique();

            opt.HasOne(x => x.Department)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
            opt.HasMany(x => x.Posts)
                .WithOne(x => x.Publisher)
                .HasForeignKey(x => x.PublisherId)
                .OnDelete(DeleteBehavior.Cascade);
            opt.HasMany(x => x.Comments)
                .WithOne(x => x.Sender)
                .HasForeignKey(x => x.SenderId)
                .OnDelete(DeleteBehavior.Cascade);
            opt.HasMany(x => x.Reactions)
                .WithOne(x => x.Sender)
                .HasForeignKey(x => x.SenderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<PostEntity>(opt =>
        {
            opt.HasKey(x => x.Id);
            opt.Property(x => x.Id).HasColumnType("uuid");
            opt.HasIndex(x => x.Id).IsUnique();
            
            opt.Property(x => x.Title).HasMaxLength(50).IsRequired();
            opt.Property(x => x.Subtitle).HasMaxLength(50);
            opt.Property(x => x.Content).IsRequired();
            opt.Property(x => x.CreatedAt).IsRequired();
            opt.Property(x => x.IsImportant).IsRequired();
            opt.Property(x => x.IsActual).IsRequired();
            opt.Property(x => x.PublisherId).IsRequired();
            
            opt.HasOne(x => x.Department)
                .WithMany(x => x.Posts)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
            opt.HasOne(x => x.Publisher)
                .WithMany(x => x.Posts)
                .HasForeignKey(x => x.PublisherId)
                .OnDelete(DeleteBehavior.Cascade);
            opt.HasMany(x => x.Reactions)
                .WithOne(x => x.Post)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            opt.HasMany(x => x.Comments)
                .WithOne(x => x.Post)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<PostReactionEntity>(opt =>
        {
            opt.HasKey(x => x.Id);
            opt.Property(x => x.Id).HasColumnType("uuid");
            opt.HasIndex(x => x.Id).IsUnique();
            
            opt.Property(x => x.PostId).IsRequired();
            opt.Property(x => x.SenderId).IsRequired();
            opt.Property(x => x.Type).IsRequired();
            opt.Property(x => x.CreatedAt).IsRequired();
            
            opt.HasOne(x => x.Post)
                .WithMany(x => x.Reactions)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            opt.HasOne(x => x.Sender)
                .WithMany(x => x.Reactions)
                .HasForeignKey(x => x.SenderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<PostCommentEntity>(opt =>
        {
            opt.HasKey(x => x.Id);
            opt.Property(x => x.Id).HasColumnType("uuid");
            opt.HasIndex(x => x.Id).IsUnique();
            
            opt.Property(x => x.Content).IsRequired();
            opt.Property(x => x.CreatedAt).IsRequired();
            opt.Property(x => x.IsDeleted).IsRequired();
            opt.Property(x => x.PostId).IsRequired();
            opt.Property(x => x.SenderId).IsRequired();
            
            opt.HasOne(x => x.Post)
                .WithMany(x => x.Comments)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);
            opt.HasOne(x => x.Sender)
                .WithMany(x => x.Comments)
                .HasForeignKey(x => x.SenderId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<DepartmentEntity>(opt =>
        {
            opt.HasKey(x => x.Id);
            opt.Property(x => x.Id).HasColumnType("uuid");
            opt.HasIndex(x => x.Id).IsUnique();
            
            opt.Property(x => x.Name).IsRequired();
            opt.HasMany(x => x.Users)
                .WithOne(x => x.Department)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
            opt.HasMany(x => x.Posts)
                .WithOne(x => x.Department)
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}