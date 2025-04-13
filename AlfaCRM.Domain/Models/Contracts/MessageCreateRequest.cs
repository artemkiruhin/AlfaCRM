namespace AlfaCRM.Domain.Models.Contracts;

public record MessageCreateRequest(string Content, Guid SenderId, Guid? RepliedMessageId, Guid ChatId);