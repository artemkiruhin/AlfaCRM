namespace AlfaCRM.Domain.Models.DTOs;

public record TicketShortDTO
(
    Guid Id,
    string Title,
    string? Feedback,
    Guid DepartmentId,
    string DepartmentName,
    DateTime CreatedAt,
    string Status,
    Guid? AssigneeId,
    string? AssigneeUsername,
    DateTime? ClosedAt,
    Guid CreatorId,
    string CreatorUsername,
    string Type
);