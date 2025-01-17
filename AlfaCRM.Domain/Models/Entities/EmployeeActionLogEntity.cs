namespace AlfaCRM.Domain.Models.Entities;

public class EmployeeActionLogEntity
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public virtual EmployeeEntity Employee { get; set; } = null!;
    public required string Description { get; set; }
    public DateTime CreatedAt { get; set; }
}