using AlfaCRM.Domain.Interfaces.Database;
using AlfaCRM.Domain.Interfaces.Services.Entity;
using AlfaCRM.Domain.Interfaces.Services.Security;
using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Services.Entity;

public class UserService : IUserService
{
    private readonly IUnitOfWork _database;
    private readonly IJwtService _jwtService;

    public UserService(IUnitOfWork database, IJwtService jwtService)
    {
        _database = database;
        _jwtService = jwtService;
    }
    
    public async Task<Result<(Guid id, string token)>> Login(LoginRequest request)
    {
        try
        {
            var user = await _database.UserRepository.GetByUsernameAndPasswordAsync(
                request.Username,
                request.PasswordHash
            );
            if (user == null) return Result<(Guid id, string token)>.Failure("User not found");
            
            var token = _jwtService.GenerateToken(user.Id);
            return Result<(Guid id, string token)>.Success((user.Id, token));
        }
        catch (Exception e)
        {
            return Result<(Guid id, string token)>.Failure($"Error while logging in: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Create(UserCreateRequest request)
    {
        await _database.BeginTransactionAsync(CancellationToken.None);
        try
        {
            var user = await _database.UserRepository.GetByUsernameAsync(request.Username);
            if (user != null) return Result<Guid>.Failure("Username already exists");
            
            var department = await _database.DepartmentRepository.GetByIdAsync(request.DepartmentId);
            if (department == null) return Result<Guid>.Failure("Department not found");

            var newUser = UserEntity.Create(
                email: request.Email,
                username: request.Username,
                passwordHash: request.PasswordHash,
                hiredAt: request.HiredAt.HasValue ? request.HiredAt.Value : DateTime.UtcNow,
                birthday: request.Birthday,
                isMale: request.IsMale,
                isAdmin: request.IsAdmin,
                hasPublishedRights: request.HasPublishedRights,
                departmentId: request.DepartmentId
            );
            
            await _database.UserRepository.CreateAsync(newUser);
            var result = await _database.SaveChangesAsync(CancellationToken.None);
            await _database.CommitTransactionAsync(CancellationToken.None);
            
            return result > 0 
                ? Result<Guid>.Success(newUser.Id) 
                : Result<Guid>.Failure("Failed to create user");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            return Result<Guid>.Failure($"Error while creating user: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Update(UserUpdateRequest request)
    {
        await _database.BeginTransactionAsync(CancellationToken.None);
        try
        {
            var user = await _database.UserRepository.GetByIdAsync(request.Id);
            if (user == null) return Result<Guid>.Failure("User not found");
            
            if (!string.IsNullOrEmpty(request.Email))
            {
                var userByEmail = await _database.UserRepository.FindAsync(u => u.Email == request.Email);
                if (userByEmail != null) return Result<Guid>.Failure("Email already exists");
                
                user.Email = request.Email;
            }
            
            if (request.IsAdmin.HasValue) user.IsAdmin = request.IsAdmin.Value;
            if (request.HasPublishedRights.HasValue) user.HasPublishedRights = request.HasPublishedRights.Value;
            
            if (request.DepartmentId.HasValue)
            {
                var department = await _database.DepartmentRepository.GetByIdAsync(request.DepartmentId.Value);
                if (department == null) return Result<Guid>.Failure("Department not found");
                user.DepartmentId = request.DepartmentId.Value;
            }
            
            _database.UserRepository.Update(user);
            var result = await _database.SaveChangesAsync(CancellationToken.None);
            await _database.CommitTransactionAsync(CancellationToken.None);
            
            return result > 0 
                ? Result<Guid>.Success(user.Id) 
                : Result<Guid>.Failure("Failed to update user");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            return Result<Guid>.Failure($"Error while updating user: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id)
    {
        await _database.BeginTransactionAsync(CancellationToken.None);
        try
        {
            var user = await _database.UserRepository.GetByIdAsync(id);
            if (user == null) return Result<Guid>.Failure("User not found");
            
            _database.UserRepository.Delete(user);
            var result = await _database.SaveChangesAsync(CancellationToken.None);
            await _database.CommitTransactionAsync(CancellationToken.None);
            
            return result > 0 
                ? Result<Guid>.Success(user.Id) 
                : Result<Guid>.Failure("Failed to delete user");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            return Result<Guid>.Failure($"Error while deleting user: {e.Message}");
        }
    }

    public async Task<Result<List<UserShortDTO>>> GetAllShort()
    {
        try
        {
            var users = await _database.UserRepository.GetAllAsync();
        
            var dtos = users.Select(user => new UserShortDTO(
                Id: user.Id,
                Username: user.Username,
                Email: user.Email,
                DepartmentName: user.Department?.Name ?? "Нет отдела"
            )).ToList();
        
            return Result<List<UserShortDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            return Result<List<UserShortDTO>>.Failure($"Error while retrieving users: {e.Message}");
        }
    }

    public async Task<Result<List<UserDetailedDTO>>> GetAll()
    {
        try
        {
            var users = await _database.UserRepository.GetAllAsync();

            var dtos = users.Select(user => new UserDetailedDTO(
                Id: user.Id,
                Username: user.Username,
                Email: user.Email,
                PasswordHash: user.PasswordHash,
                HiredAt: user.HiredAt,
                FiredAt: user.FiredAt,
                Birthday: user.Birthday,
                IsMale: user.IsMale,
                IsActive: user.IsActive,
                IsAdmin: user.IsAdmin,
                HasPublishedRights: user.HasPublishedRights,
                IsBlocked: user.IsBlocked,
                Department: user.DepartmentId.HasValue
                    ? new DepartmentShortDTO(
                        Id: user.DepartmentId.Value,
                        Name: user.Department.Name
                    )
                    : null,
                Posts: user.Posts.Select(post => new PostShortDTO(
                    Id: post.Id,
                    Title: post.Title
                )).ToList()
            )).ToList();
        
            return Result<List<UserDetailedDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            return Result<List<UserDetailedDTO>>.Failure($"Error while retrieving users: {e.Message}");
        }
    }

    public async Task<Result<UserDetailedDTO>> GetById(Guid id)
    {
        try
        {
            var user = await _database.UserRepository.GetByIdAsync(id);
            if (user == null) return Result<UserDetailedDTO>.Failure("User not found");

            var dto = new UserDetailedDTO(
                Id: user.Id,
                Username: user.Username,
                Email: user.Email,
                PasswordHash: user.PasswordHash,
                HiredAt: user.HiredAt,
                FiredAt: user.FiredAt,
                Birthday: user.Birthday,
                IsMale: user.IsMale,
                IsActive: user.IsActive,
                IsAdmin: user.IsAdmin,
                HasPublishedRights: user.HasPublishedRights,
                IsBlocked: user.IsBlocked,
                Department: user.DepartmentId.HasValue
                    ? new DepartmentShortDTO(
                        Id: user.DepartmentId.Value,
                        Name: user.Department.Name
                    )
                    : null,
                Posts: user.Posts.Select(post => new PostShortDTO(
                    Id: post.Id,
                    Title: post.Title
                )).ToList()
            );
        
            return Result<UserDetailedDTO>.Success(dto);
        }
        catch (Exception e)
        {
            return Result<UserDetailedDTO>.Failure($"Error while retrieving user: {e.Message}");
        }
    }

    public async Task<Result<UserShortDTO>> GetByIdShort(Guid id)
    {
        try
        {
            var user = await _database.UserRepository.GetByIdAsync(id);
            if (user == null) return Result<UserShortDTO>.Failure("User not found");
        
            var dto = new UserShortDTO(
                Id: user.Id,
                Username: user.Username,
                Email: user.Email,
                DepartmentName: user.Department?.Name ?? "Нет отдела"
            );
        
            return Result<UserShortDTO>.Success(dto);
        }
        catch (Exception e)
        {
            return Result<UserShortDTO>.Failure($"Error while retrieving user: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Block(Guid id)
    {
        await _database.BeginTransactionAsync(CancellationToken.None);
        try
        {
            var user = await _database.UserRepository.GetByIdAsync(id);
            if (user == null) return Result<Guid>.Failure("User not found");
            
            user.IsBlocked = true;
            
            _database.UserRepository.Update(user);
            var result = await _database.SaveChangesAsync(CancellationToken.None);
            await _database.CommitTransactionAsync(CancellationToken.None);
            
            return result > 0 
                ? Result<Guid>.Success(user.Id) 
                : Result<Guid>.Failure("Failed to block user");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            return Result<Guid>.Failure($"Error while blocking user: {e.Message}");
        }
    }

    public async Task<Result<Guid>> ResetPassword(Guid id, string oldPassword, string newPassword)
    {
        await _database.BeginTransactionAsync(CancellationToken.None);
        
        if (oldPassword == newPassword) return Result<Guid>.Failure("New password must be different from the old one");
        try
        {
            var user = await _database.UserRepository.GetByIdAsync(id);
            if (user == null) return Result<Guid>.Failure("User not found");
            
            if (user.PasswordHash != oldPassword) return Result<Guid>.Failure("Old password is incorrect");
            user.PasswordHash = newPassword;
            
            _database.UserRepository.Update(user);
            var result = await _database.SaveChangesAsync(CancellationToken.None);
            await _database.CommitTransactionAsync(CancellationToken.None);
            
            return result > 0 
                ? Result<Guid>.Success(user.Id) 
                : Result<Guid>.Failure("Failed to reset password");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            return Result<Guid>.Failure($"Error while resetting password: {e.Message}");
        }
    }

    public async Task<Result<Guid>> ResetPasswordAsAdmin(Guid id, string newPassword)
    {
        await _database.BeginTransactionAsync(CancellationToken.None);
        
        try
        {
            var user = await _database.UserRepository.GetByIdAsync(id);
            if (user == null) return Result<Guid>.Failure("User not found");
            
            user.PasswordHash = newPassword;
            
            _database.UserRepository.Update(user);
            var result = await _database.SaveChangesAsync(CancellationToken.None);
            await _database.CommitTransactionAsync(CancellationToken.None);
            
            return result > 0 
                ? Result<Guid>.Success(user.Id) 
                : Result<Guid>.Failure("Failed to reset password");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            return Result<Guid>.Failure($"Error while resetting password: {e.Message}");
        }
    }
}