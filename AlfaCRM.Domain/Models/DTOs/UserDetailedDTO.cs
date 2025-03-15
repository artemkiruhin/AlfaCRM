namespace AlfaCRM.Domain.Models.DTOs;

public record UserDetailedDTO(
    Guid Id,
    string Username,
    string Email,
    string PasswordHash,
    DateTime HiredAt,
    DateTime? FiredAt,
    DateTime Birthday,
    bool IsMale,
    bool IsActive,
    bool IsAdmin,
    bool HasPublishedRights,
    bool IsBlocked,
    DepartmentShortDTO? Department,
    List<PostShortDTO> Posts
);