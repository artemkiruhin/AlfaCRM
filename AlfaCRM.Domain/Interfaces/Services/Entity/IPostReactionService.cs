using AlfaCRM.Domain.Models.Contracts;

namespace AlfaCRM.Domain.Interfaces.Services.Entity;

public interface IPostReactionService
{
    Task<bool> Create(PostReactionCreateRequest request);
    Task<bool> Delete(Guid id);
}