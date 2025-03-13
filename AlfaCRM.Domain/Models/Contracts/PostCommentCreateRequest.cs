namespace AlfaCRM.Domain.Models.Contracts;

public record PostCommentCreateRequest(
    string Content,
    Guid PostId,
    Guid SenderId
);