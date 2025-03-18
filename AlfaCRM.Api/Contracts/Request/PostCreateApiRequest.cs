namespace AlfaCRM.Api.Contracts.Request;

public record PostCreateApiRequest(
    string Title,
    string? Subtitle,
    string Content,
    bool IsImportant,
    Guid? DepartmentId
    );