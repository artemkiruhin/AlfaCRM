using AlfaCRM.Domain.Models.DTOs;

namespace AlfaCRM.Domain.Interfaces.Services.Entity;

public interface IDepartmentService
{
    Task<IEnumerable<DepartmentDto>> GetAllAsync(Guid senderId);
    Task<DepartmentDto?> GetByIdAsync(Guid senderId, Guid id);
    Task<DepartmentDto?> GetByNameAsync(Guid senderId, string name);
    Task<bool> AddAsync(Guid senderId, string name);
    Task<bool> UpdateAsync(Guid senderId, Guid id, string name);
}