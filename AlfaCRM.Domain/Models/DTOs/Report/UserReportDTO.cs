namespace AlfaCRM.Domain.Models.DTOs.Report;

public record UserReportDTO(
    Guid Id,
    string FullName,
    string Username,
    string Email,
    DateTime HiredAt,
    DateTime? FiredAt,
    DateTime Birthday,
    string Sex,
    bool IsAdmin,
    bool HasPublishedRights,
    bool IsBlocked,
    string DepartmentGuid,
    string DepartmentName,
    int PublishedPostsCount
);