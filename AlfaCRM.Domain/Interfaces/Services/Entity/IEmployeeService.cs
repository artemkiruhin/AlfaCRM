using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;

namespace AlfaCRM.Domain.Interfaces.Services.Entity;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDetailedDto>> GetAllAsync(Guid senderId);
    Task<EmployeeDetailedDto?> GetByIdAsync(Guid senderId, Guid id);
    Task<EmployeeDetailedDto?> GetByUsernameAsync(Guid senderId, string username);
    Task<EmployeeDetailedDto?> GetByEmailAsync(Guid senderId, string email);
    Task<EmployeeDetailedDto?> GetByPhoneNumberAsync(Guid senderId, string phoneNumber);
    Task<bool> AddAsync(Guid senderId, EmployeeCreateContract request);
    Task<bool> UpdateAsync(Guid senderId, EmployeeUpdateContract request);
    Task<IEnumerable<EmployeeDetailedDto>> GetFilteredAsync(Guid userId, EmployeeQueryParameters parameters);
}