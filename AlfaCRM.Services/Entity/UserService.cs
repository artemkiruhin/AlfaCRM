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
    
    public async Task<(Guid id, string token)> Login(LoginRequest request)
    {
        try
        {
            var user = await _database.UserRepository.GetByUsernameAndPasswordAsync(
                request.Username,
                request.PasswordHash
            );
            if (user == null) throw new KeyNotFoundException();
            
            var token = _jwtService.GenerateToken(user.Id);
            return (user.Id, token);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<bool> Create(UserCreateRequest request)
    {
        await _database.BeginTransactionAsync(CancellationToken.None);
        try
        {
            var user = await _database.UserRepository.GetByUsernameAsync(request.Username);
            if (user != null) throw new InvalidOperationException();
            
            var department = await _database.DepartmentRepository.GetByIdAsync(request.DepartmentId);
            if (department == null) throw new KeyNotFoundException();

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
            
            return result > 0;
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<bool> Update(UserUpdateRequest request)
    {
        await _database.BeginTransactionAsync(CancellationToken.None);
        try
        {
            var user = await _database.UserRepository.GetByIdAsync(request.Id);
            if (user == null) throw new KeyNotFoundException();
            
            if (!string.IsNullOrEmpty(request.Email))
            {
                var userByEmail = await _database.UserRepository.FindAsync(u => u.Email == request.Email);
                if (userByEmail != null) throw new ArgumentException();
                
                user.Email = request.Email;
            }
            
            if (request.IsAdmin.HasValue) user.IsAdmin = request.IsAdmin.Value;
            if (request.HasPublishedRights.HasValue) user.HasPublishedRights = request.HasPublishedRights.Value;
            
            if (request.DepartmentId.HasValue)
            {
                var department = await _database.DepartmentRepository.GetByIdAsync(request.DepartmentId.Value);
                if (department == null) throw new KeyNotFoundException();
                user.DepartmentId = request.DepartmentId.Value;
            }
            
            _database.UserRepository.Update(user);
            var result = await _database.SaveChangesAsync(CancellationToken.None);
            await _database.CommitTransactionAsync(CancellationToken.None);
            
            return result > 0;
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<bool> Delete(Guid id)
    {
        await _database.BeginTransactionAsync(CancellationToken.None);
        try
        {
            var user = await _database.UserRepository.GetByIdAsync(id);
            if (user == null) throw new KeyNotFoundException();
            
            _database.UserRepository.Delete(user);
            var result = await _database.SaveChangesAsync(CancellationToken.None);
            await _database.CommitTransactionAsync(CancellationToken.None);
            
            return result > 0;
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<List<UserShortDTO>> GetAllShort()
    {
        var users = await _database.UserRepository.GetAllAsync();
        
        var dtos = users.Select(user => new UserShortDTO(
            Id: user.Id,
            Username: user.Username,
            Email: user.Email,
            DepartmentName: user.Department?.Name ?? "Нет отдела"
        )).ToList();
        
        return dtos;
    }

    public async Task<List<UserDetailedDTO>> GetAll()
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
        
        return dtos;
    }

    public async Task<UserDetailedDTO> GetById(Guid id)
    {
        var user = await _database.UserRepository.GetByIdAsync(id);
        if (user == null) throw new KeyNotFoundException();

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
        
        return dto;
    }

    public async Task<UserShortDTO> GetByIdShort(Guid id)
    {
        var user = await _database.UserRepository.GetByIdAsync(id);
        if (user == null) throw new KeyNotFoundException();
        
        var dto = new UserShortDTO(
            Id: user.Id,
            Username: user.Username,
            Email: user.Email,
            DepartmentName: user.Department?.Name ?? "Нет отдела"
        );
        
        return dto;
    }

    public async Task<bool> Block(Guid id)
    {
        await _database.BeginTransactionAsync(CancellationToken.None);
        try
        {
            var user = await _database.UserRepository.GetByIdAsync(id);
            if (user == null) throw new KeyNotFoundException();
            
            user.IsBlocked = true;
            
            _database.UserRepository.Update(user);
            var result = await _database.SaveChangesAsync(CancellationToken.None);
            await _database.CommitTransactionAsync(CancellationToken.None);
            
            return result > 0;
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<bool> ResetPassword(Guid id, string oldPassword, string newPassword)
    {
        await _database.BeginTransactionAsync(CancellationToken.None);
        
        if (oldPassword == newPassword) throw new ArgumentException();
        try
        {
            var user = await _database.UserRepository.GetByIdAsync(id);
            if (user == null) throw new KeyNotFoundException();
            
            if (user.PasswordHash != oldPassword) throw new ArgumentException();
            user.PasswordHash = newPassword;
            
            _database.UserRepository.Update(user);
            var result = await _database.SaveChangesAsync(CancellationToken.None);
            await _database.CommitTransactionAsync(CancellationToken.None);
            
            return result > 0;
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<bool> ResetPasswordAsAdmin(Guid id, string newPassword)
    {
        await _database.BeginTransactionAsync(CancellationToken.None);
        
        try
        {
            var user = await _database.UserRepository.GetByIdAsync(id);
            if (user == null) throw new KeyNotFoundException();
            
            user.PasswordHash = newPassword;
            
            _database.UserRepository.Update(user);
            var result = await _database.SaveChangesAsync(CancellationToken.None);
            await _database.CommitTransactionAsync(CancellationToken.None);
            
            return result > 0;
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(CancellationToken.None);
            Console.WriteLine(e);
            throw;
        }
    }
}