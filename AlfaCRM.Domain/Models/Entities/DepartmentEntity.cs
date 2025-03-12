namespace AlfaCRM.Domain.Models.Entities;

public class DepartmentEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public virtual ICollection<UserEntity> Users { get; set; } = [];
    public virtual ICollection<PostEntity> Posts { get; set; } = [];
}