﻿using AlfaCRM.Domain.Interfaces.Database;
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

    public async Task<Result<Guid>> Create(DepartmentCreateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);

        try
        {
            var dbDepartment = await _database.DepartmentRepository.GetDepartmentByName(request.Name, ct);
            if (dbDepartment != null) return Result<Guid>.Failure($"Department {request.Name} already exists");
            
            var newDepartment = DepartmentEntity.Create(request.Name);
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
            var dbDepartment = await _database.DepartmentRepository.GetByIdAsync(request.DepartmentId, ct);
            if (dbDepartment == null) return Result<Guid>.Failure("Department not found");
            
            dbDepartment.Name = request.Name;
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
            var dtos = entities.Select(department => new DepartmentShortDTO(
                Id: department.Id,
                Name: department.Name
            )).ToList();

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
            
            var departmentUsers = entity.Users.Select(user => new UserShortDTO(
                Id: user.Id,
                Username: user.Username,
                Email: user.Email,
                DepartmentName: entity.Name
            )).ToList();

            var dto = new DepartmentDetailedDTO(
                Id: entity.Id,
                Name: entity.Name,
                Users: departmentUsers
            );

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
            
            var dto = new DepartmentShortDTO(
                Id: entity.Id,
                Name: entity.Name
            );

            return Result<DepartmentShortDTO>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<DepartmentShortDTO>.Failure($"Error while retrieving department: {ex.Message}");
        }
    }
}