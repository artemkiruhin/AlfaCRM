namespace AlfaCRM.Domain.Models.DTOs.Report;

public record TicketReportDTO
(
    Guid Id,
    string Title,
    string Text,
    string Feedback,
    string DepartmentGuid,
    string DepartmentName,
    DateTime CreatedAt,
    string Status,
    string AssigneeUsername,
    string AssigneeFullName,
    string CreatorUsername,
    string CreatorFullName,
    DateTime? ClosedAt,
    string Type
);