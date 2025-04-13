namespace AlfaCRM.Domain.Models.Contracts;

public record MessageUpdateRequest(Guid Id, string? Content);