using AlfaCRM.Domain.Models.Entities;
using AlfaCRM.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace AlfaCRM.Infrastructure;

public class AppDbContext : DbContext
{
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<PostEntity> Posts { get; set; }
    public DbSet<PostReactionEntity> PostReactions { get; set; }
    public DbSet<PostCommentEntity> PostComments { get; set; }
    public DbSet<DepartmentEntity> Departments { get; set; }
    public DbSet<TicketEntity> Tickets { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfiguration(new PostReactionConfiguration());
        modelBuilder.ApplyConfiguration(new PostCommentConfiguration());
        modelBuilder.ApplyConfiguration(new PostConfiguration());
        modelBuilder.ApplyConfiguration(new DepartmentConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new TicketConfiguration());
    }
}