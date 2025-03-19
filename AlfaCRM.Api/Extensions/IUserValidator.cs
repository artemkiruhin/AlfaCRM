using System.Security.Claims;
using AlfaCRM.Domain.Models.DTOs;

namespace AlfaCRM.Api.Extensions;

public interface IUserValidator
{
    Task<Result<bool>> IsAdminOrPublisher(ClaimsPrincipal user, CancellationToken ct);
    Task<Result<bool>> IsAdmin(ClaimsPrincipal user, CancellationToken ct);
    Task<Result<Guid>> GetUserId(ClaimsPrincipal user, CancellationToken ct);
}