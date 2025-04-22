using AlfaCRM.Domain.Interfaces.Database;
using AlfaCRM.Domain.Interfaces.Services.Entity;
using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Services.Entity;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _database;

    public DepartmentService(IUnitOfWork database)
    {
        _database = database;
    }

    private DepartmentShortDTO MapShort(DepartmentEntity entity)
    {
        return new DepartmentShortDTO(
            Id: entity.Id,
            Name: entity.Name,
            MembersCount: entity.Users.Count,
            IsSpecific: entity.IsSpecific
        );
    }

    private List<DepartmentShortDTO> MapShortRange(IEnumerable<DepartmentEntity> entities)
    {
        return entities.Select(entitiy => new DepartmentShortDTO(
            Id: entitiy.Id,
            Name: entitiy.Name,
            MembersCount: entitiy.Users.Count,
            IsSpecific: entitiy.IsSpecific
        )).ToList();
    }
    
    private DepartmentDetailedDTO MapDetailed(DepartmentEntity entity)
    {
        return new DepartmentDetailedDTO(
            Id: entity.Id,
            Name: entity.Name,
            IsSpecific: entity.IsSpecific,
            Users: entity.Users.Select(user => new UserShortDTO(
                Id: user.Id,
                FullName: user.FullName,
                Username: user.Username,
                Email: user.Email,
                DepartmentName: user.Department?.Name ?? "Нет отдела",
                IsAdmin: user?.IsAdmin ?? false,
                IsBlocked: user?.IsBlocked ?? false
            )).ToList()
        );
    }

    private List<DepartmentDetailedDTO> MapDetailedRange(IEnumerable<DepartmentEntity> entities)
    {
        return entities.Select(department => new DepartmentDetailedDTO(
            Id: department.Id,
            Name: department.Name,
            IsSpecific: department.IsSpecific,
            Users: department.Users.Select(user => new UserShortDTO(
                Id: user.Id,
                FullName: user.FullName,
                Username: user.Username,
                Email: user.Email,
                DepartmentName: department.Name,
                IsAdmin: user?.IsAdmin ?? false,
                IsBlocked: user?.IsBlocked ?? false
            )).ToList()
        )).ToList();
    }

    
    public async Task<Result<Guid>> Create(DepartmentCreateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);

        try
        {
            var dbDepartment = await _database.DepartmentRepository.GetDepartmentByName(request.Name, ct);
            if (dbDepartment != null) return Result<Guid>.Failure($"Department {request.Name} already exists");
            
            var newDepartment = DepartmentEntity.Create(request.Name, request.IsSpecific);
            await _database.DepartmentRepository.CreateAsync(newDepartment, ct);
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            var department = await _database.DepartmentRepository.GetByIdAsync(newDepartment.Id, ct);
            if (department == null) return Result<Guid>.Failure("Department not found");
            
            return Result<Guid>.Success(department.Id);
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<Guid>.Failure($"Error while creating department: {ex.Message}");
        }
    }

    public async Task<Result<Guid>> Update(DepartmentUpdateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);

        try
        {
            if (!request.IsSpecific.HasValue && string.IsNullOrEmpty(request.Name)) 
                return Result<Guid>.Failure("At least 1 field is required");
            
            var dbDepartment = await _database.DepartmentRepository.GetByIdAsync(request.DepartmentId, ct);
            if (dbDepartment == null) return Result<Guid>.Failure("Department not found");
            
            if (!string.IsNullOrEmpty(request.Name) && dbDepartment.Name != request.Name) 
                dbDepartment.Name = request.Name;
            if (request.IsSpecific.HasValue && request.IsSpecific != dbDepartment.IsSpecific) 
                dbDepartment.IsSpecific = request.IsSpecific.Value;
            _database.DepartmentRepository.Update(dbDepartment, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            return result > 0 
                ? Result<Guid>.Success(dbDepartment.Id) 
                : Result<Guid>.Failure("Failed to update department");
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<Guid>.Failure($"Error while updating department: {ex.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);

        try
        {
            var dbDepartment = await _database.DepartmentRepository.GetByIdAsync(id, ct);
            if (dbDepartment == null) return Result<Guid>.Failure("Department not found");
            
            _database.DepartmentRepository.Delete(dbDepartment, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            return result > 0 
                ? Result<Guid>.Success(dbDepartment.Id) 
                : Result<Guid>.Failure("Failed to delete department");
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<Guid>.Failure($"Error while deleting department: {ex.Message}");
        }
    }

    public async Task<Result<List<DepartmentShortDTO>>> GetAllShort(CancellationToken ct)
    {
        try
        {
            var entities = await _database.DepartmentRepository.GetAllAsync(ct);
            var dtos = MapShortRange(entities);
            return Result<List<DepartmentShortDTO>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<DepartmentShortDTO>>.Failure($"Error while retrieving departments: {ex.Message}");
        }
    }

    public async Task<Result<List<DepartmentDetailedDTO>>> GetAll(CancellationToken ct)
    {
        try
        {
            var entities = await _database.DepartmentRepository.GetAllAsync(ct);
            var dtos = MapDetailedRange(entities);
            return Result<List<DepartmentDetailedDTO>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<List<DepartmentDetailedDTO>>.Failure($"Error while retrieving departments: {ex.Message}");
        }
    }

    public async Task<Result<DepartmentDetailedDTO>> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var entity = await _database.DepartmentRepository.GetByIdAsync(id, ct);
            if (entity == null) return Result<DepartmentDetailedDTO>.Failure("Department not found");
            
            var dto = MapDetailed(entity);
            return Result<DepartmentDetailedDTO>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<DepartmentDetailedDTO>.Failure($"Error while retrieving department: {ex.Message}");
        }
    }

    public async Task<Result<DepartmentShortDTO>> GetByIdShort(Guid id, CancellationToken ct)
    {
        try
        {
            var entity = await _database.DepartmentRepository.GetByIdAsync(id, ct);
            if (entity == null) return Result<DepartmentShortDTO>.Failure("Department not found");

            var dto = MapShort(entity);
            return Result<DepartmentShortDTO>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<DepartmentShortDTO>.Failure($"Error while retrieving department: {ex.Message}");
        }
    }
}