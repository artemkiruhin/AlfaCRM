namespace AlfaCRM.Api.Contracts.Response;

public record MessageResponse(Guid Id, string Content, string CreatedAt, string Username, bool IsOwn);