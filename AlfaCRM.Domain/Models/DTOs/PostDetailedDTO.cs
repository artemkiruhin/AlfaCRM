namespace AlfaCRM.Domain.Models.DTOs;

public record PostDetailedDTO(
    Guid Id,
    string Title,
    string? Subtitle,
    string Content,
    DateTime CreatedAt,
    DateTime? ModifiedAt,
    bool IsImportant,
    bool IsActual,
    UserShortDTO Publisher,
    DepartmentShortDTO Department,
    List<PostReactionShortDTO> Reactions,
    List<PostCommentShortDTO> Comments
);