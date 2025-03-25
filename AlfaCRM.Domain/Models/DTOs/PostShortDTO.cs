namespace AlfaCRM.Domain.Models.DTOs;

public record PostShortDTO(
    Guid Id,
    string Title,
    DateTime CreatedAt,
    bool IsImportant,
    string Department,
    Guid? DepartmentId
);