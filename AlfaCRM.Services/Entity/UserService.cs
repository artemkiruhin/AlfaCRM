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
    private readonly IHashService _hasher;

    public UserService(IUnitOfWork database, IJwtService jwtService, IHashService hasher)
    {
        _database = database;
        _jwtService = jwtService;
        _hasher = hasher;
    }

    private UserShortDTO MapToShortDTO(UserEntity entity)
    {
        return new UserShortDTO(
            Id: entity.Id,
            Username: entity.Username,
            Email: entity.Email,
            DepartmentName: entity.Department?.Name ?? "Нет отдела"
        );
    }

    private List<UserShortDTO> MapToShortDTORange(IEnumerable<UserEntity> entities)
    {
        return entities.Select(entity => new UserShortDTO(
            Id: entity.Id,
            Username: entity.Username,
            Email: entity.Email,
            DepartmentName: entity.Department?.Name ?? "Нет отдела"
        )).ToList();
    }

    private UserDetailedDTO MapToDetailedDTO(UserEntity entity)
    {
        return new UserDetailedDTO(
            Id: entity.Id,
            Username: entity.Username,
            Email: entity.Email,
            PasswordHash: entity.PasswordHash,
            HiredAt: entity.HiredAt,
            FiredAt: entity.FiredAt,
            Birthday: entity.Birthday,
            IsMale: entity.IsMale,
            IsActive: entity.IsActive,
            IsAdmin: entity.IsAdmin,
            HasPublishedRights: entity.HasPublishedRights,
            IsBlocked: entity.IsBlocked,
            Department: entity.DepartmentId.HasValue
                ? new DepartmentShortDTO(
                    Id: entity.DepartmentId.Value,
                    Name: entity.Department.Name
                )
                : null,
            Posts: entity.Posts.Select(post => new PostShortDTO(
                Id: post.Id,
                Title: post.Title,
                CreatedAt: post.CreatedAt,
                IsImportant: post.IsImportant,
                Department: post.Department?.Name ?? "Общая новость",
                DepartmentId: entity.DepartmentId
            )).ToList()
        );
    }

    private List<UserDetailedDTO> MapToDetailedDTORange(IEnumerable<UserEntity> entities)
    {
        return entities.Select(entity => new UserDetailedDTO(
            Id: entity.Id,
            Username: entity.Username,
            Email: entity.Email,
            PasswordHash: entity.PasswordHash,
            HiredAt: entity.HiredAt,
            FiredAt: entity.FiredAt,
            Birthday: entity.Birthday,
            IsMale: entity.IsMale,
            IsActive: entity.IsActive,
            IsAdmin: entity.IsAdmin,
            HasPublishedRights: entity.HasPublishedRights,
            IsBlocked: entity.IsBlocked,
            Department: entity.DepartmentId.HasValue
                ? new DepartmentShortDTO(
                    Id: entity.DepartmentId.Value,
                    Name: entity.Department.Name
                )
                : null,
            Posts: entity.Posts.Select(post => new PostShortDTO(
                Id: post.Id,
                Title: post.Title,
                CreatedAt: post.CreatedAt,
                IsImportant: post.IsImportant,
                Department: post.Department?.Name ?? "Общая новость",
                DepartmentId: entity.DepartmentId
            )).ToList()
        )).ToList();
    }

    public async Task<Result<(Guid id, string username, string token)>> Login(LoginRequest request, CancellationToken ct)
    {
        try
        {
            var user = await _database.UserRepository.GetByUsernameAndPasswordAsync(
                request.Username,
                _hasher.ComputeHash(request.PasswordHash),
                ct
            );
            if (user == null) return Result<(Guid id, string username, string token)>.Failure("User not found");

            var token = _jwtService.GenerateToken(user.Id);
            return Result<(Guid id, string username, string token)>.Success((user.Id, user.Username, token));
        }
        catch (Exception e)
        {
            return Result<(Guid id, string username, string token)>.Failure($"Error while logging in: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Create(UserCreateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            var user = await _database.UserRepository.GetByUsernameAsync(request.Username, ct);
            if (user != null) return Result<Guid>.Failure("Username already exists");

            var department = await _database.DepartmentRepository.GetByIdAsync(request.DepartmentId, ct);
            if (department == null) return Result<Guid>.Failure("Department not found");

            var newUser = UserEntity.Create(
                email: request.Email,
                username: request.Username,
                passwordHash: _hasher.ComputeHash(request.PasswordHash),
                hiredAt: request.HiredAt.HasValue ? request.HiredAt.Value : DateTime.UtcNow,
                birthday: request.Birthday,
                isMale: request.IsMale,
                isAdmin: request.IsAdmin,
                hasPublishedRights: request.HasPublishedRights,
                departmentId: request.DepartmentId
            );

            await _database.UserRepository.CreateAsync(newUser, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            return result > 0
                ? Result<Guid>.Success(newUser.Id)
                : Result<Guid>.Failure("Failed to create user");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<Guid>.Failure($"Error while creating user: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Update(UserUpdateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            var user = await _database.UserRepository.GetByIdAsync(request.Id, ct);
            if (user == null) return Result<Guid>.Failure("User not found");

            if (!string.IsNullOrEmpty(request.Email))
            {
                var userByEmail = await _database.UserRepository.FindAsync(u => u.Email == request.Email, ct);
                if (userByEmail != null) return Result<Guid>.Failure("Email already exists");

                user.Email = request.Email;
            }

            if (request.IsAdmin.HasValue) user.IsAdmin = request.IsAdmin.Value;
            if (request.HasPublishedRights.HasValue) user.HasPublishedRights = request.HasPublishedRights.Value;

            if (request.DepartmentId.HasValue)
            {
                var department = await _database.DepartmentRepository.GetByIdAsync(request.DepartmentId.Value, ct);
                if (department == null) return Result<Guid>.Failure("Department not found");
                user.DepartmentId = request.DepartmentId.Value;
            }

            _database.UserRepository.Update(user, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            return result > 0
                ? Result<Guid>.Success(user.Id)
                : Result<Guid>.Failure("Failed to update user");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<Guid>.Failure($"Error while updating user: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            var user = await _database.UserRepository.GetByIdAsync(id, ct);
            if (user == null) return Result<Guid>.Failure("User not found");

            _database.UserRepository.Delete(user, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            return result > 0
                ? Result<Guid>.Success(user.Id)
                : Result<Guid>.Failure("Failed to delete user");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<Guid>.Failure($"Error while deleting user: {e.Message}");
        }
    }

    public async Task<Result<List<UserShortDTO>>> GetAllShort(CancellationToken ct)
    {
        try
        {
            var users = await _database.UserRepository.GetAllAsync(ct);
            var dtos = MapToShortDTORange(users);
            return Result<List<UserShortDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            return Result<List<UserShortDTO>>.Failure($"Error while retrieving users: {e.Message}");
        }
    }

    public async Task<Result<List<UserDetailedDTO>>> GetAll(CancellationToken ct)
    {
        try
        {
            var users = await _database.UserRepository.GetAllAsync(ct);
            var dtos = MapToDetailedDTORange(users);
            return Result<List<UserDetailedDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            return Result<List<UserDetailedDTO>>.Failure($"Error while retrieving users: {e.Message}");
        }
    }

    public async Task<Result<UserDetailedDTO>> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var user = await _database.UserRepository.GetByIdAsync(id, ct);
            if (user == null) return Result<UserDetailedDTO>.Failure("User not found");

            var dto = MapToDetailedDTO(user);
            return Result<UserDetailedDTO>.Success(dto);
        }
        catch (Exception e)
        {
            return Result<UserDetailedDTO>.Failure($"Error while retrieving user: {e.Message}");
        }
    }

    public async Task<Result<UserShortDTO>> GetByIdShort(Guid id, CancellationToken ct)
    {
        try
        {
            var user = await _database.UserRepository.GetByIdAsync(id, ct);
            if (user == null) return Result<UserShortDTO>.Failure("User not found");

            var dto = MapToShortDTO(user);
            return Result<UserShortDTO>.Success(dto);
        }
        catch (Exception e)
        {
            return Result<UserShortDTO>.Failure($"Error while retrieving user: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Block(Guid id, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            var user = await _database.UserRepository.GetByIdAsync(id, ct);
            if (user == null) return Result<Guid>.Failure("User not found");

            user.IsBlocked = true;

            _database.UserRepository.Update(user, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            return result > 0
                ? Result<Guid>.Success(user.Id)
                : Result<Guid>.Failure("Failed to block user");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<Guid>.Failure($"Error while blocking user: {e.Message}");
        }
    }

    public async Task<Result<Guid>> ResetPassword(Guid id, string oldPassword, string newPassword, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);

        if (oldPassword == newPassword) return Result<Guid>.Failure("New password must be different from the old one");
        try
        {
            var user = await _database.UserRepository.GetByIdAsync(id, ct);
            if (user == null) return Result<Guid>.Failure("User not found");

            var oldPasswordHashed = _hasher.ComputeHash(oldPassword);
            var newPasswordHashed = _hasher.ComputeHash(newPassword);
            
            if (user.PasswordHash != oldPasswordHashed) return Result<Guid>.Failure("Old password is incorrect");
            user.PasswordHash = newPasswordHashed;

            _database.UserRepository.Update(user, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            return result > 0
                ? Result<Guid>.Success(user.Id)
                : Result<Guid>.Failure("Failed to reset password");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<Guid>.Failure($"Error while resetting password: {e.Message}");
        }
    }

    public async Task<Result<Guid>> ResetPasswordAsAdmin(Guid id, string newPassword, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);

        try
        {
            var user = await _database.UserRepository.GetByIdAsync(id, ct);
            if (user == null) return Result<Guid>.Failure("User not found");

            user.PasswordHash = _hasher.ComputeHash(newPassword);

            _database.UserRepository.Update(user, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            return result > 0
                ? Result<Guid>.Success(user.Id)
                : Result<Guid>.Failure("Failed to reset password");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<Guid>.Failure($"Error while resetting password: {e.Message}");
        }
    }
}