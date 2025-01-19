namespace AlfaCRM.Domain.Models.DTOs;

public class NotWorkingDayBidDto
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
    public required string Type { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime DayFrom { get; set; }
    public DateTime DayTo { get; set; }
    public Guid FileId { get; set; }
    //public required  File { get; set; } = null!;
}