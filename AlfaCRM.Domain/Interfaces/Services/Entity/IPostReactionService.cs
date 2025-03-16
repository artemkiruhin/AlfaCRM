using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;

namespace AlfaCRM.Domain.Interfaces.Services.Entity;

public interface IPostReactionService
{
    Task<Result<Guid>> Create(PostReactionCreateRequest request);
    Task<Result<Guid>> Delete(Guid id);
}