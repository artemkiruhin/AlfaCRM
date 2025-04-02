﻿namespace AlfaCRM.Domain.Models.Entities;

public class DepartmentEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public bool IsSpecific { get; set; }

    public virtual ICollection<UserEntity> Users { get; set; } = [];
    public virtual ICollection<PostEntity> Posts { get; set; } = [];
    public virtual ICollection<TicketEntity> Tickets { get; set; } = [];
    public static DepartmentEntity Create(string name, bool isSpecific)
    {
        return new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            IsSpecific = isSpecific
        };
    }
}