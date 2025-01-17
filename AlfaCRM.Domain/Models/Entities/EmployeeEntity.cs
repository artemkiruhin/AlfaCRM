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
}
