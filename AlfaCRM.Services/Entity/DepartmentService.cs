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
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting department creation process. Request: {System.Text.Json.JsonSerializer.Serialize(request)}",
                null), ct);

            var dbDepartment = await _database.DepartmentRepository.GetDepartmentByName(request.Name, ct);
            if (dbDepartment != null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to create department: department with name '{request.Name}' already exists",
                    null), ct);
                return Result<Guid>.Failure($"Department {request.Name} already exists");
            }
            
            var newDepartment = DepartmentEntity.Create(request.Name, request.IsSpecific);
            await _database.DepartmentRepository.CreateAsync(newDepartment, ct);
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Department created successfully with ID: {newDepartment.Id}",
                null), ct);

            var department = await _database.DepartmentRepository.GetByIdAsync(newDepartment.Id, ct);
            if (department == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Error,
                    $"Department with ID {newDepartment.Id} not found after creation",
                    null), ct);
                return Result<Guid>.Failure("Department not found");
            }
            
            return Result<Guid>.Success(department.Id);
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while creating department: {ex.Message}. StackTrace: {ex.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Error while creating department: {ex.Message}");
        }
    }

    public async Task<Result<Guid>> Update(DepartmentUpdateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting department update process for ID {request.DepartmentId}. Request: {System.Text.Json.JsonSerializer.Serialize(request)}",
                null), ct);

            if (!request.IsSpecific.HasValue && string.IsNullOrEmpty(request.Name))
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    "Failed to update department: at least 1 field is required",
                    null), ct);
                return Result<Guid>.Failure("At least 1 field is required");
            }
            
            var dbDepartment = await _database.DepartmentRepository.GetByIdAsync(request.DepartmentId, ct);
            if (dbDepartment == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to update department: department with ID {request.DepartmentId} not found",
                    null), ct);
                return Result<Guid>.Failure("Department not found");
            }
            
            if (!string.IsNullOrEmpty(request.Name) && dbDepartment.Name != request.Name) 
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Updating department {request.DepartmentId} name from '{dbDepartment.Name}' to '{request.Name}'",
                    null), ct);
                dbDepartment.Name = request.Name;
            }
            
            if (request.IsSpecific.HasValue && request.IsSpecific != dbDepartment.IsSpecific) 
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Updating department {request.DepartmentId} IsSpecific from {dbDepartment.IsSpecific} to {request.IsSpecific.Value}",
                    null), ct);
                dbDepartment.IsSpecific = request.IsSpecific.Value;
            }
            
            _database.DepartmentRepository.Update(dbDepartment, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Department {request.DepartmentId} updated successfully",
                    null), ct);
                return Result<Guid>.Success(dbDepartment.Id);
            }
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"No changes were made to department {request.DepartmentId}",
                null), ct);
            return Result<Guid>.Failure("Failed to update department");
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while updating department {request?.DepartmentId}: {ex.Message}. StackTrace: {ex.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Error while updating department: {ex.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting department deletion process for ID {id}",
                null), ct);

            var dbDepartment = await _database.DepartmentRepository.GetByIdAsync(id, ct);
            if (dbDepartment == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to delete department: department with ID {id} not found",
                    null), ct);
                return Result<Guid>.Failure("Department not found");
            }
            
            _database.DepartmentRepository.Delete(dbDepartment, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Department {id} deleted successfully",
                    null), ct);
                return Result<Guid>.Success(dbDepartment.Id);
            }
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"No changes were made when trying to delete department {id}",
                null), ct);
            return Result<Guid>.Failure("Failed to delete department");
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while deleting department {id}: {ex.Message}. StackTrace: {ex.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Error while deleting department: {ex.Message}");
        }
    }

    public async Task<Result<List<DepartmentShortDTO>>> GetAllShort(CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                "Starting to retrieve all departments (short version)",
                null), ct);

            var entities = await _database.DepartmentRepository.GetAllAsync(ct);
            var dtos = MapShortRange(entities);
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Retrieved {dtos.Count} departments (short version)",
                null), ct);
            
            return Result<List<DepartmentShortDTO>>.Success(dtos);
        }
        catch (Exception ex)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while retrieving all departments (short version): {ex.Message}. StackTrace: {ex.StackTrace}",
                null), ct);
            return Result<List<DepartmentShortDTO>>.Failure($"Error while retrieving departments: {ex.Message}");
        }
    }

    public async Task<Result<List<DepartmentDetailedDTO>>> GetAll(CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                "Starting to retrieve all departments (detailed version)",
                null), ct);

            var entities = await _database.DepartmentRepository.GetAllAsync(ct);
            var dtos = MapDetailedRange(entities);
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Retrieved {dtos.Count} departments (detailed version)",
                null), ct);
            
            return Result<List<DepartmentDetailedDTO>>.Success(dtos);
        }
        catch (Exception ex)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while retrieving all departments (detailed version): {ex.Message}. StackTrace: {ex.StackTrace}",
                null), ct);
            return Result<List<DepartmentDetailedDTO>>.Failure($"Error while retrieving departments: {ex.Message}");
        }
    }

    public async Task<Result<DepartmentDetailedDTO>> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to retrieve department (detailed) with ID {id}",
                null), ct);

            var entity = await _database.DepartmentRepository.GetByIdAsync(id, ct);
            if (entity == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Department with ID {id} not found",
                    null), ct);
                return Result<DepartmentDetailedDTO>.Failure("Department not found");
            }
            
            var dto = MapDetailed(entity);
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Successfully retrieved department (detailed) with ID {id}",
                null), ct);
            
            return Result<DepartmentDetailedDTO>.Success(dto);
        }
        catch (Exception ex)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while retrieving department (detailed) with ID {id}: {ex.Message}. StackTrace: {ex.StackTrace}",
                null), ct);
            return Result<DepartmentDetailedDTO>.Failure($"Error while retrieving department: {ex.Message}");
        }
    }

    public async Task<Result<DepartmentShortDTO>> GetByIdShort(Guid id, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to retrieve department (short) with ID {id}",
                null), ct);

            var entity = await _database.DepartmentRepository.GetByIdAsync(id, ct);
            if (entity == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Department with ID {id} not found",
                    null), ct);
                return Result<DepartmentShortDTO>.Failure("Department not found");
            }

            var dto = MapShort(entity);
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Successfully retrieved department (short) with ID {id}",
                null), ct);
            
            return Result<DepartmentShortDTO>.Success(dto);
        }
        catch (Exception ex)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while retrieving department (short) with ID {id}: {ex.Message}. StackTrace: {ex.StackTrace}",
                null), ct);
            return Result<DepartmentShortDTO>.Failure($"Error while retrieving department: {ex.Message}");
        }
    }

    public async Task<Result<int>> GetDepartmentCount(CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                "Starting to count departments",
                null), ct);

            var departmentCount = await _database.DepartmentRepository.CountAsync(ct);
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Department count completed: {departmentCount} departments found",
                null), ct);
            
            return Result<int>.Success(departmentCount);
        }
        catch (Exception ex)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while counting departments: {ex.Message}. StackTrace: {ex.StackTrace}",
                null), ct);
            return Result<int>.Failure($"Error while counting departments: {ex.Message}");
        }
    }
}