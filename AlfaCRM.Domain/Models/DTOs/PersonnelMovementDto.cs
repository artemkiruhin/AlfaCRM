namespace AlfaCRM.Domain.Models.DTOs;

public class PersonnelMovementDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public required string EmployeeUsername { get; set; }
    public required string EmployeeName { get; set; }
    public required string EmployeeSurname { get; set; }
    public required string EmployeePatronymic { get; set; }
    public required string EmployeeEmail { get; set; }
    public required string EmployeePhoneNumber { get; set; }
    public bool EmployeeHasManagementRights { get; set; }
    public Guid HRId { get; set; }
    public required string HRUsername { get; set; }
    public required string HRName { get; set; }
    public required string HRSurname { get; set; }
    public required string HRPatronymic { get; set; }
    public required string HREmail { get; set; }
    public required string HRPhoneNumber { get; set; }
    public Guid? PositionFromId { get; set; }
    public Guid? PositionToId { get; set; }
    public required string PositionNameFrom { get; set; } 
    public required string PositionNameTo { get; set; } 
    public DateTime CreatedAt { get; set; }
}