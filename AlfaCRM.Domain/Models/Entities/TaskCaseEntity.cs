namespace AlfaCRM.Domain.Models.Entities;

public class TaskCaseEntity
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public virtual EmployeeEntity Employee { get; set; } = null!;
    
    public Guid HelperId { get; set; }
    public virtual EmployeeEntity Helper { get; set; } = null!;
    
    public TaskCaseStatusEntity Status { get; set; }

    
    
    public Guid TypeId { get; set; }
    
    
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public bool IsClosed => ClosedAt.HasValue;
}