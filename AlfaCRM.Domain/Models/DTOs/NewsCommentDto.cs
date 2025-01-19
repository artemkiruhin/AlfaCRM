namespace AlfaCRM.Domain.Models.DTOs;

public class NewsCommentDto
{
    public Guid Id { get; set; }
    public required string Content { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid? RepliedCommentId { get; set; }
    public string? RepliedCommentContent { get; set; }
    public DateTime CreatedAt { get; set; }
    public required string EmployeeUsername { get; set; }
    public required string EmployeeName { get; set; }
    public required string EmployeeSurname { get; set; }
    public required string EmployeePatronymic { get; set; }
    public required string EmployeeEmail { get; set; }
    public required string EmployeePhoneNumber { get; set; }
    public bool EmployeeHasManagementRights { get; set; }
}