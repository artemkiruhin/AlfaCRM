namespace AlfaCRM.Domain.Models.DTOs;

public record PostCommentShortDTO(
    Guid Id,
    string Content,
    bool IsDeleted,
    DateTime CreatedAt,
    UserShortDTO Sender
);