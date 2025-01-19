namespace AlfaCRM.Domain.Models.DTOs;

public record TaskCaseCommentDto
{
    public Guid Id { get; init; }
    public required string Content { get; init; }
    public required EmployeeShortDto Publisher { get; init; }
    public DateTime CreatedAt { get; init; }
}