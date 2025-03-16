using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;

namespace AlfaCRM.Domain.Interfaces.Services.Entity;

public interface IUserService
{
    Task<Result<(Guid id, string token)>> Login(LoginRequest request);
    
    Task<Result<Guid>> Create(UserCreateRequest request);
    Task<Result<Guid>> Update(UserUpdateRequest request);
    Task<Result<Guid>> Delete(Guid id);
    
    Task<Result<List<UserShortDTO>>> GetAllShort();
    Task<Result<List<UserDetailedDTO>>> GetAll();
    Task<Result<UserDetailedDTO>> GetById(Guid id);
    Task<Result<UserShortDTO>> GetByIdShort(Guid id);

    Task<Result<Guid>> Block(Guid id);
    Task<Result<Guid>> ResetPassword(Guid id, string oldPassword, string newPassword);
    Task<Result<Guid>> ResetPasswordAsAdmin(Guid id, string newPassword);
}