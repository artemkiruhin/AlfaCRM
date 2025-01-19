namespace AlfaCRM.Domain.Models.DTOs;

public record NewsDto
{
    public Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Content { get; init; }
    public int Watchers { get; init; }
    public required EmployeeShortDto Publisher { get; init; }
    public required IReadOnlyList<DepartmentDto> Departments { get; init; } = [];
    public required IReadOnlyList<NewsCommentDto> Comments { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}