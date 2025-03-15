namespace AlfaCRM.Domain.Models.Contracts;

public record UserCreateRequest(
    string Email,
    string Username,
    string PasswordHash,
    DateTime? HiredAt,
    DateTime Birthday,
    bool IsMale,
    bool IsAdmin,
    bool HasPublishedRights,
    Guid DepartmentId
);