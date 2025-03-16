using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;

namespace AlfaCRM.Domain.Interfaces.Services.Entity;

public interface IUserService
{
    Task<Result<(Guid id, string token)>> Login(LoginRequest request, CancellationToken ct);
    
    Task<Result<Guid>> Create(UserCreateRequest request, CancellationToken ct);
    Task<Result<Guid>> Update(UserUpdateRequest request, CancellationToken ct);
    Task<Result<Guid>> Delete(Guid id, CancellationToken ct);
    
    Task<Result<List<UserShortDTO>>> GetAllShort(CancellationToken ct);
    Task<Result<List<UserDetailedDTO>>> GetAll(CancellationToken ct);
    Task<Result<UserDetailedDTO>> GetById(Guid id, CancellationToken ct);
    Task<Result<UserShortDTO>> GetByIdShort(Guid id, CancellationToken ct);

    Task<Result<Guid>> Block(Guid id, CancellationToken ct);
    Task<Result<Guid>> ResetPassword(Guid id, string oldPassword, string newPassword, CancellationToken ct);
    Task<Result<Guid>> ResetPasswordAsAdmin(Guid id, string newPassword, CancellationToken ct);
}