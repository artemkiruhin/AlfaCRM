using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;

namespace AlfaCRM.Domain.Interfaces.Services.Entity;

public interface IPostReactionService
{
    Task<Result<Guid>> Create(PostReactionCreateRequest request, CancellationToken ct);
    Task<Result<Guid>> Delete(Guid id, CancellationToken ct);
    Task<Result<bool>> DeleteAll(Guid postId, Guid userId, CancellationToken ct);
}