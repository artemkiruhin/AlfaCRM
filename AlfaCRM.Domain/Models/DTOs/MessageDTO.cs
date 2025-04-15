namespace AlfaCRM.Domain.Models.DTOs;

public record MessageDTO(
    Guid Id,
    string Content,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? DeletedAt,
    bool IsDeleted,
    bool IsPinned,
    UserShortDTO? Sender,
    MessageDTO? RepliedMessage,
    List<MessageDTO> Replies,
    bool IsOwn
);