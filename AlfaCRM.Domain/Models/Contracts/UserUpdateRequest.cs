namespace AlfaCRM.Domain.Models.Contracts;

public record UserUpdateRequest(
    Guid Id,
    string? Email,
    bool? IsAdmin,
    bool? HasPublishedRights,
    Guid? DepartmentId
);