namespace AlfaCRM.Domain.Models.DTOs;

public record DepartmentShortDTO(
    Guid Id,
    string Name,
    int MembersCount,
    bool IsSpecific
);