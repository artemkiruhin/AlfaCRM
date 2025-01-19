namespace AlfaCRM.Domain.Models.DTOs;

public record NewsCommentDto
{
    public Guid Id { get; init; }
    public required string Content { get; init; }
    public required EmployeeShortDto Employee { get; init; }
    public Guid? RepliedCommentId { get; init; }
    public string? RepliedCommentContent { get; init; }
    public DateTime CreatedAt { get; init; }
}