namespace AlfaCRM.Domain.Models.DTOs;

public record TicketDetailedDTO
(
    Guid Id,
    string Title,
    string Text,
    string Feedback,
    DepartmentShortDTO Department,
    DateTime CreatedAt,
    string Status,
    UserShortDTO Assignee,
    DateTime? ClosedAt
);