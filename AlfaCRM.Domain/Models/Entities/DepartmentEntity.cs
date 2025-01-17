namespace AlfaCRM.Domain.Models.Entities;

public class DepartmentEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public virtual ICollection<EmployeeEntity> Employees { get; set; } = [];
    public virtual ICollection<NewsEntity> News { get; set; } = [];
}