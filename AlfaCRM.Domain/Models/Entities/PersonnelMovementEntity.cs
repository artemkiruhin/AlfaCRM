namespace AlfaCRM.Domain.Models.Entities;

public class PersonnelMovementEntity
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public virtual EmployeeEntity Employee { get; set; } = null!;
    
    public Guid HRId { get; set; }
    public virtual EmployeeEntity HR { get; set; } = null!;
    
    public Guid? PositionFromId { get; set; }
    public Guid? PositionToId { get; set; }
    
    public virtual PositionEntity? PositionFrom { get; set; } 
    public virtual PositionEntity? PositionTo { get; set; } 
    
    public DateTime CreatedAt { get; set; }
}