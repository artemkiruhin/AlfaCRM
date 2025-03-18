namespace AlfaCRM.Api.Contracts.Request;

public record PostCommentCreateApiRequest(
    string Content,
    Guid PostId
    );