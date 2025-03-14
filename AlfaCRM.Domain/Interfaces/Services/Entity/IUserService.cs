using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;

namespace AlfaCRM.Domain.Interfaces.Services.Entity;

public interface IUserService
{
    Task<(Guid id, string token)> Login(LoginRequest request);
    
    Task<bool> Create(UserCreateRequest request);
    Task<bool> Update(UserUpdateRequest request);
    Task<bool> Delete(Guid id);
    
    Task<List<UserShortDTO>> GetAllShort();
    Task<List<UserDetailedDTO>> GetAll();
    Task<UserDetailedDTO> GetById(Guid id);
    Task<UserShortDTO> GetByIdShort(Guid id);

    Task<bool> Block(Guid id);
    Task<bool> ResetPassword(Guid id, string oldPassword, string newPassword);
    Task<bool> ResetPasswordAsAdmin(Guid id, string newPassword);
}