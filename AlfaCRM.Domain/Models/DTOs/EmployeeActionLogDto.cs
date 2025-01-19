namespace AlfaCRM.Domain.Models.DTOs;

public record EmployeeActionLogDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public required string EmployeeUsername { get; set; } 
    public required string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public required string EmployeeName { get; set; }
    public required string EmployeeSurname { get; set; }
    public required string EmployeePatronymic { get; set; }
    public required string EmployeeEmail { get; set; }
    public required string EmployeePhoneNumber { get; set; }
    public bool EmployeeHasManagementRights { get; set; }
    public required string EmployeePosition { get; set; }
    public required string EmployeeDepartment { get; set; }
}