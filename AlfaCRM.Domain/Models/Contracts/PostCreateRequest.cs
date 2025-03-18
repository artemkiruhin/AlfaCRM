namespace AlfaCRM.Domain.Models.Contracts;

public record PostCreateRequest(
    string Title,
    string? Subtitle,
    string Content,
    bool IsImportant,
    Guid? DepartmentId,
    Guid PublisherId
);