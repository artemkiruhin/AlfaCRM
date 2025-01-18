namespace AlfaCRM.Domain.Models.Entities;

public class VacancyEntity
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal Salary { get; set; }
    public bool IsSalaryGross { get; set; }
    public ShiftType ShiftType { get; set; }
    public int WorkTimeFrom { get; set; }
    public int WorkTimeTo { get; set; }
    public bool AllowsRemoteWork { get; set; }
    public int ExperienceFrom { get; set; }
    public int ExperienceTo { get; set; }
    public int FixedExperience { get; set; }
    public int Watchers { get; set; }
    public DateTime CreatedAt { get; set; }
    public VacancyType Type { get; set; }
    
    public Guid PublisherId { get; set; }
    public virtual EmployeeEntity Publisher { get; set; } = null!;
    
    public virtual ICollection<VacancyReplyEntity> Replies { get; set; } = [];
}