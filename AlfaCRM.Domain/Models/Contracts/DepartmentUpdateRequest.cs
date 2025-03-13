namespace AlfaCRM.Domain.Models.Contracts;

public record DepartmentUpdateRequest(
    Guid DepartmentId,
    string Name
);