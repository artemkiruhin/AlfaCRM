namespace AlfaCRM.Domain.Models.Entities;

public class PositionEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public decimal Salary { get; set; }
    public bool IsSalaryGross { get; set; }
    
    public virtual ICollection<EmployeeEntity> Employees { get; set; } = [];
}