namespace AlfaCRM.Domain.Models.DTOs;

public record DepartmentShortDTO(
    Guid Id,
    string Name,
    bool IsSpecific
);