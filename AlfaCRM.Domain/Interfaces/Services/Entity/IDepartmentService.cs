using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;

namespace AlfaCRM.Domain.Interfaces.Services.Entity;

public interface IDepartmentService
{
    Task<Result<Guid>> Create(DepartmentCreateRequest request, CancellationToken ct);
    Task<Result<Guid>> Update(DepartmentUpdateRequest request, CancellationToken ct);
    Task<Result<Guid>> Delete(Guid id, CancellationToken ct);
    
    Task<Result<List<DepartmentShortDTO>>> GetAllShort(CancellationToken ct);
    Task<Result<List<DepartmentDetailedDTO>>> GetAll(CancellationToken ct);
    Task<Result<DepartmentDetailedDTO>> GetById(Guid id, CancellationToken ct);
    Task<Result<DepartmentShortDTO>> GetByIdShort(Guid id, CancellationToken ct);
    Task<Result<int>> GetDepartmentCount(CancellationToken ct);
}