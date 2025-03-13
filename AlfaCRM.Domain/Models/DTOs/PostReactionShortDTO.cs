namespace AlfaCRM.Domain.Models.DTOs;

public record PostReactionShortDTO(
    Guid Id,
    UserShortDTO Sender,
    DateTime CreatedAt,
    string Type
);