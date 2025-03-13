using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Models.Contracts;

public record PostReactionCreateRequest(
    Guid PostId,
    Guid SenderId,
    ReactionType Type
);