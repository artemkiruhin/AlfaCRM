namespace AlfaCRM.Domain.Models.DTOs.Report;

public record LogReportDTO(
    Guid Id,
    string Message,
    string Type,
    string UserIdString,
    string Username,
    DateTime CreatedAt
);