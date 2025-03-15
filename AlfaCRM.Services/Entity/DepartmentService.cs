using AlfaCRM.Domain.Interfaces.Database;
using AlfaCRM.Domain.Interfaces.Services.Entity;
using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;
using AlfaCRM.Domain.Models.Entities;
using Microsoft.Extensions.Logging;

namespace AlfaCRM.Services.Entity;

public class DepartmentService : IDepartmentService
{
    private readonly IUnitOfWork _database;

    public DepartmentService(IUnitOfWork database)
    {
        _database = database;
    }

    public async Task<bool> Create(DepartmentCreateRequest request)
    {
        await _database.BeginTransactionAsync(CancellationToken.None);

        try
        {
            var dbDepartment = await _database.DepartmentRepository.GetDepartmentByName(request.Name);
            if (dbDepartment != null) throw new InvalidOperationException();
            
            var newDepartment = DepartmentEntity.Create(request.Name);
            await _database.DepartmentRepository.CreateAsync(newDepartment);
            var result = await _database.SaveChangesAsync(CancellationToken.None);
            await _database.CommitTransactionAsync(CancellationToken.None);

            return result > 0;
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<bool> Update(DepartmentUpdateRequest request)
    {
        await _database.BeginTransactionAsync(CancellationToken.None);

        try
        {
            var dbDepartment = await _database.DepartmentRepository.GetByIdAsync(request.DepartmentId);
            if (dbDepartment == null) throw new KeyNotFoundException();
            
            dbDepartment.Name = request.Name;
            _database.DepartmentRepository.Update(dbDepartment);
            var result = await _database.SaveChangesAsync(CancellationToken.None);
            await _database.CommitTransactionAsync(CancellationToken.None);

            return result > 0;
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<bool> Delete(Guid id)
    {
        await _database.BeginTransactionAsync(CancellationToken.None);

        try
        {
            var dbDepartment = await _database.DepartmentRepository.GetByIdAsync(id);
            if (dbDepartment == null) throw new KeyNotFoundException();
            
            _database.DepartmentRepository.Delete(dbDepartment);
            var result = await _database.SaveChangesAsync(CancellationToken.None);
            await _database.CommitTransactionAsync(CancellationToken.None);

            return result > 0;
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<List<DepartmentShortDTO>> GetAllShort()
    {
        try
        {
            var entities = await _database.DepartmentRepository.GetAllAsync();
            var dtos = entities.Select(department => new DepartmentShortDTO(
                Id: department.Id,
                Name: department.Name
            )).ToList();

            return dtos;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<List<DepartmentDetailedDTO>> GetAll()
    {
        try
        {
            var entities = await _database.DepartmentRepository.GetAllAsync();
            var dtos = entities.Select(department =>
            {
                var departmentUsers = department.Users.Select(user => new UserShortDTO(
                    Id: user.Id,
                    Username: user.Username,
                    Email: user.Email,
                    DepartmentName: department.Name
                )).ToList();

                return new DepartmentDetailedDTO(
                    Id: department.Id,
                    Name: department.Name,
                    Users: departmentUsers
                );
            }).ToList();

            return dtos;
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<DepartmentDetailedDTO> GetById(Guid id)
    {
        try
        {
            var entity = await _database.DepartmentRepository.GetByIdAsync(id);
            if (entity == null) throw new KeyNotFoundException();
            
            var departmentUsers = entity.Users.Select(user => new UserShortDTO(
                Id: user.Id,
                Username: user.Username,
                Email: user.Email,
                DepartmentName: entity.Name
            )).ToList();

            return new DepartmentDetailedDTO(
                Id: entity.Id,
                Name: entity.Name,
                Users: departmentUsers
            );
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<DepartmentShortDTO> GetByIdShort(Guid id)
    {
        try
        {
            var entity = await _database.DepartmentRepository.GetByIdAsync(id);
            if (entity == null) throw new KeyNotFoundException();
            
            return new DepartmentShortDTO(
                Id: entity.Id,
                Name: entity.Name
            );
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}