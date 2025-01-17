using System.Runtime.InteropServices.JavaScript;

namespace AlfaCRM.Domain.Models.Entities;

public class EmployeeEntity
{
    public Guid Id { get; set; }
    public required string Username { get; set; }
    public required string PasswordHash { get; set; }
    public required string Name { get; set; }
    public required string Surname { get; set; }
    public string? Patronymic { get; set; }
    public string? Email { get; set; }
    public required string PhoneNumber { get; set; }
    public bool HasManagementRights { get; set; }
    public DateTime Birthday { get; set; }
    public DateTime HiredAt { get; set; }
    public DateTime? FiredAt { get; set; }
    public bool IsFired => FiredAt.HasValue;
    public int Age => DateTime.Now.Year - Birthday.Year - (DateTime.Now.DayOfYear < Birthday.DayOfYear ? 1 : 0);
    
    public Guid PositionId { get; set; }
    public Guid DepartmentId { get; set; }

    public virtual ICollection<NotWorkingDayBidEntity> NotWorkingDayBids { get; set; } = [];
    public virtual ICollection<PersonnelMovementEntity> PersonnelMovements { get; set; } = [];
    public virtual ICollection<NewsEntity> News { get; set; } = [];
    public virtual ICollection<NewsCommentEntity> NewsComments { get; set; } = [];
    public virtual ICollection<TaskCaseCommentEntity> TasksComments { get; set; } = [];
    public virtual ICollection<TaskCaseEntity> Tasks { get; set; } = [];
    public virtual ICollection<EmployeeActionLogEntity> Actions { get; set; } = [];
    
    public virtual PositionEntity Position { get; set; } = null!;
    public virtual DepartmentEntity Department { get; set; } = null!;
}