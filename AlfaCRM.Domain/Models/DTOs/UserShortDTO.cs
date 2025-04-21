namespace AlfaCRM.Domain.Models.DTOs;

public record UserShortDTO(
    Guid Id,
    string Username,
    string Email,
    string DepartmentName,
    bool IsAdmin,
    bool IsBlocked
);