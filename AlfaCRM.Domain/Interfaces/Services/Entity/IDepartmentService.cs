using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;

namespace AlfaCRM.Domain.Interfaces.Services.Entity;

public interface IDepartmentService
{
    Task<bool> Create(DepartmentCreateRequest request);
    Task<bool> Update(DepartmentUpdateRequest request);
    Task<bool> Delete(Guid id);
    
    Task<List<DepartmentShortDTO>> GetAllShort();
    Task<List<DepartmentDetailedDTO>> GetAll();
    Task<DepartmentDetailedDTO> GetById(Guid id);
    Task<DepartmentShortDTO> GetByIdShort(Guid id);
}