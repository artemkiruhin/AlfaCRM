namespace AlfaCRM.Domain.Models.Contracts;

public record PostUpdateRequest(
    Guid PostId,
    string? Title,
    string? Subtitle,
    string? Content,
    bool? IsImportant,
    Guid? DepartmentId,
    bool EditDepartment
);