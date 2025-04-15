namespace AlfaCRM.Api.Contracts.Response;

public record MessageResponse(Guid Id, string Content, DateTime Timestamp, string Username, bool IsOwn);