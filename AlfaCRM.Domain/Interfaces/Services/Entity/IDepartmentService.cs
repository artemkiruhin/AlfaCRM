using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;

namespace AlfaCRM.Domain.Interfaces.Services.Entity;

public interface IDepartmentService
{
    Task<Result<Guid>> Create(DepartmentCreateRequest request);
    Task<Result<Guid>> Update(DepartmentUpdateRequest request);
    Task<Result<Guid>> Delete(Guid id);
    
    Task<Result<List<DepartmentShortDTO>>> GetAllShort();
    Task<Result<List<DepartmentDetailedDTO>>> GetAll();
    Task<Result<DepartmentDetailedDTO>> GetById(Guid id);
    Task<Result<DepartmentShortDTO>> GetByIdShort(Guid id);
}