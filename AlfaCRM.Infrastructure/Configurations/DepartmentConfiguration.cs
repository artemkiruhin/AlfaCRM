using AlfaCRM.Domain.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlfaCRM.Infrastructure.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<DepartmentEntity>
{
    public void Configure(EntityTypeBuilder<DepartmentEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("uuid");
        builder.HasIndex(x => x.Id).IsUnique();
            
        builder.Property(x => x.Name).IsRequired();
        builder.HasMany(x => x.Users)
            .WithOne(x => x.Department)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasMany(x => x.Posts)
            .WithOne(x => x.Department)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class TicketConfiguration : IEntityTypeConfiguration<TicketEntity>
{
    public void Configure(EntityTypeBuilder<TicketEntity> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnType("uuid");
        builder.HasIndex(x => x.Id).IsUnique();
            
        builder.Property(x => x.Title).IsRequired();
        builder.Property(x => x.Text).IsRequired();
        builder.Property(x => x.CreatorId).IsRequired();
        builder.Property(x => x.DepartmentId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        
        builder.HasOne(x => x.Creator)
            .WithMany(x => x.Tickets)
            .HasForeignKey(x => x.CreatorId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Department)
            .WithMany(x => x.Tickets)
            .HasForeignKey(x => x.DepartmentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Assignee)
            .WithMany(x => x.Tickets)
            .HasForeignKey(x => x.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}