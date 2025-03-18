using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Api.Contracts.Request;

public record PostReactionCreateApiRequest(
    Guid PostId,
    ReactionType Type
    );