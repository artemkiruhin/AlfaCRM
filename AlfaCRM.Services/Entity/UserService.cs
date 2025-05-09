using System.Globalization;
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
            FullName: entity.FullName,
            Username: entity.Username,
            Email: entity.Email,
            DepartmentName: entity.Department?.Name ?? "Нет отдела",
            IsAdmin: entity.IsAdmin,
            IsBlocked: entity.IsBlocked
        );
    }

    private List<UserShortDTO> MapToShortDTORange(IEnumerable<UserEntity> entities)
    {
        return entities.Select(entity => new UserShortDTO(
            Id: entity.Id,
            FullName: entity.FullName,
            Username: entity.Username,
            Email: entity.Email,
            DepartmentName: entity.Department?.Name ?? "Нет отдела",
            IsAdmin: entity.IsAdmin,
            IsBlocked: entity.IsBlocked
        )).ToList();
    }

    private UserDetailedDTO MapToDetailedDTO(UserEntity entity)
    {
        return new UserDetailedDTO(
            Id: entity.Id,
            FullName: entity.FullName,
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
                    Name: entity.Department.Name,
                    MembersCount: entity.Department.Users.Count,
                    IsSpecific: entity.Department.IsSpecific
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

    public async Task<Result<(Guid id, string username, Guid departmentId, bool isSpecDepartment, bool isAdmin, string token)>> Login(LoginRequest request, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting login process for username: {request.Username}",
                null), ct);

            var user = await _database.UserRepository.GetByUsernameAndPasswordAsync(
                request.Username,
                _hasher.ComputeHash(request.PasswordHash),
                ct
            );

            if (user == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Login failed for username: {request.Username} - user not found or invalid credentials",
                    null), ct);
                return Result<(Guid id, string username, Guid departmentId, bool isSpecDepartment, bool isAdmin, string token)>.Failure("Invalid username or password");
            }

            if (user.IsBlocked)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Login blocked for user {user.Id} (username: {user.Username}) - account is blocked",
                    user.Id), ct);
                return Result<(Guid id, string username, Guid departmentId, bool isSpecDepartment, bool isAdmin, string token)>.Failure("Account is blocked");
            }

            var token = _jwtService.GenerateToken(user.Id);
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"User {user.Id} (username: {user.Username}) logged in successfully",
                user.Id), ct);

            return Result<(Guid id, string username, Guid departmentId, bool isSpecDepartment, bool isAdmin, string token)>
                .Success((user.Id, user.Username, user.DepartmentId ?? Guid.Empty, user.Department?.IsSpecific ?? false, user.IsAdmin, token));
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error during login for username {request.Username}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<(Guid id, string username, Guid departmentId, bool isSpecDepartment, bool isAdmin, string token)>.Failure($"Error while logging in: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Create(UserCreateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting user creation process. Request: {System.Text.Json.JsonSerializer.Serialize(request)}",
                null), ct);

            var existingUser = await _database.UserRepository.GetByUsernameAsync(request.Username, ct);
            if (existingUser != null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to create user: username {request.Username} already exists",
                    null), ct);
                return Result<Guid>.Failure("Username already exists");
            }

            var department = await _database.DepartmentRepository.GetByIdAsync(request.DepartmentId, ct);
            if (department == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to create user: department with ID {request.DepartmentId} not found",
                    null), ct);
                return Result<Guid>.Failure("Department not found");
            }

            var newUser = UserEntity.Create(
                fullName: request.FullName,
                email: request.Email,
                username: request.Username,
                passwordHash: _hasher.ComputeHash(request.PasswordHash),
                hiredAt: request.HiredAt ?? DateTime.UtcNow,
                birthday: request.Birthday,
                isMale: request.IsMale,
                isAdmin: request.IsAdmin,
                hasPublishedRights: request.HasPublishedRights,
                departmentId: request.DepartmentId
            );

            await _database.UserRepository.CreateAsync(newUser, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"User created successfully with ID: {newUser.Id}",
                    newUser.Id), ct);
                return Result<Guid>.Success(newUser.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                "No changes were made when creating user",
                null), ct);
            return Result<Guid>.Failure("Failed to create user");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while creating user: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Error while creating user: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Update(UserUpdateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting user update process for ID {request.Id}. Request: {System.Text.Json.JsonSerializer.Serialize(request)}",
                null), ct);

            var user = await _database.UserRepository.GetByIdAsync(request.Id, ct);
            if (user == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to update user: user with ID {request.Id} not found",
                    null), ct);
                return Result<Guid>.Failure("User not found");
            }

            if (!string.IsNullOrEmpty(request.Email))
            {
                var userByEmail = await _database.UserRepository.FindAsync(u => u.Email == request.Email && u.Id != request.Id, ct);
                if (userByEmail != null)
                {
                    await _database.LogRepository.CreateAsync(LogEntity.Create(
                        LogType.Warning,
                        $"Failed to update user: email {request.Email} already exists",
                        null), ct);
                    return Result<Guid>.Failure("Email already exists");
                }
                user.Email = request.Email;
            }

            if (request.IsAdmin.HasValue) 
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Updating IsAdmin from {user.IsAdmin} to {request.IsAdmin.Value} for user {request.Id}",
                    null), ct);
                user.IsAdmin = request.IsAdmin.Value;
            }

            if (request.HasPublishedRights.HasValue) 
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Updating HasPublishedRights from {user.HasPublishedRights} to {request.HasPublishedRights.Value} for user {request.Id}",
                    null), ct);
                user.HasPublishedRights = request.HasPublishedRights.Value;
            }

            if (request.DepartmentId.HasValue)
            {
                var department = await _database.DepartmentRepository.GetByIdAsync(request.DepartmentId.Value, ct);
                if (department == null)
                {
                    await _database.LogRepository.CreateAsync(LogEntity.Create(
                        LogType.Warning,
                        $"Failed to update user: department with ID {request.DepartmentId} not found",
                        null), ct);
                    return Result<Guid>.Failure("Department not found");
                }
                user.DepartmentId = request.DepartmentId.Value;
            }

            _database.UserRepository.Update(user, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"User {request.Id} updated successfully",
                    request.Id), ct);
                return Result<Guid>.Success(user.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"No changes were made when updating user {request.Id}",
                null), ct);
            return Result<Guid>.Failure("Failed to update user");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while updating user {request.Id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Error while updating user: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting user deletion process for ID {id}",
                null), ct);

            var user = await _database.UserRepository.GetByIdAsync(id, ct);
            if (user == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to delete user: user with ID {id} not found",
                    null), ct);
                return Result<Guid>.Failure("User not found");
            }

            // Log related entities that will be affected
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Deleting user {id} with {user.Posts.Count} posts, {user.Comments.Count} comments, and {user.Messages.Count} messages",
                null), ct);

            _database.UserRepository.Delete(user, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"User {id} deleted successfully",
                    null), ct);
                return Result<Guid>.Success(user.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"No changes were made when deleting user {id}",
                null), ct);
            return Result<Guid>.Failure("Failed to delete user");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while deleting user {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Error while deleting user: {e.Message}");
        }
    }

    public async Task<Result<List<UserShortDTO>>> GetAllShort(CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                "Starting to retrieve all users (short version)",
                null), ct);

            var users = await _database.UserRepository.GetAllAsync(ct);
            var dtos = MapToShortDTORange(users);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Retrieved {dtos.Count} users (short version)",
                null), ct);

            return Result<List<UserShortDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while retrieving users (short version): {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<List<UserShortDTO>>.Failure($"Error while retrieving users: {e.Message}");
        }
    }

    public async Task<Result<List<UserDetailedDTO>>> GetAll(CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                "Starting to retrieve all users (detailed version)",
                null), ct);

            var users = await _database.UserRepository.GetAllAsync(ct);
            var dtos = users.Select(MapToDetailedDTO).ToList();

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Retrieved {dtos.Count} users (detailed version)",
                null), ct);

            return Result<List<UserDetailedDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while retrieving users (detailed version): {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<List<UserDetailedDTO>>.Failure($"Error while retrieving users: {e.Message}");
        }
    }

    public async Task<Result<UserDetailedDTO>> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to retrieve user (detailed) with ID {id}",
                null), ct);

            var user = await _database.UserRepository.GetByIdAsync(id, ct);
            if (user == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"User with ID {id} not found",
                    null), ct);
                return Result<UserDetailedDTO>.Failure("User not found");
            }

            var dto = MapToDetailedDTO(user);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Successfully retrieved user (detailed) with ID {id}",
                null), ct);

            return Result<UserDetailedDTO>.Success(dto);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while retrieving user (detailed) {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<UserDetailedDTO>.Failure($"Error while retrieving user: {e.Message}");
        }
    }

    public async Task<Result<UserShortDTO>> GetByIdShort(Guid id, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to retrieve user (short) with ID {id}",
                null), ct);

            var user = await _database.UserRepository.GetByIdAsync(id, ct);
            if (user == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"User with ID {id} not found",
                    null), ct);
                return Result<UserShortDTO>.Failure("User not found");
            }

            var dto = MapToShortDTO(user);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Successfully retrieved user (short) with ID {id}",
                null), ct);

            return Result<UserShortDTO>.Success(dto);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while retrieving user (short) {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<UserShortDTO>.Failure($"Error while retrieving user: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Block(Guid id, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to block user with ID {id}",
                null), ct);

            var user = await _database.UserRepository.GetByIdAsync(id, ct);
            if (user == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to block user: user with ID {id} not found",
                    null), ct);
                return Result<Guid>.Failure("User not found");
            }

            if (user.IsBlocked)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"User {id} is already blocked",
                    null), ct);
                return Result<Guid>.Failure("User is already blocked");
            }

            user.IsBlocked = true;

            _database.UserRepository.Update(user, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"User {id} blocked successfully",
                    null), ct);
                return Result<Guid>.Success(user.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"No changes were made when blocking user {id}",
                null), ct);
            return Result<Guid>.Failure("Failed to block user");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while blocking user {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Error while blocking user: {e.Message}");
        }
    }

    public async Task<Result<Guid>> ResetPassword(Guid id, string oldPassword, string newPassword, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting password reset process for user {id}",
                null), ct);

            if (oldPassword == newPassword)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    "New password must be different from the old one",
                    null), ct);
                return Result<Guid>.Failure("New password must be different from the old one");
            }

            var user = await _database.UserRepository.GetByIdAsync(id, ct);
            if (user == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to reset password: user with ID {id} not found",
                    null), ct);
                return Result<Guid>.Failure("User not found");
            }

            var oldPasswordHashed = _hasher.ComputeHash(oldPassword);
            var newPasswordHashed = _hasher.ComputeHash(newPassword);
            
            if (user.PasswordHash != oldPasswordHashed)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    "Old password is incorrect",
                    null), ct);
                return Result<Guid>.Failure("Old password is incorrect");
            }

            user.PasswordHash = newPasswordHashed;

            _database.UserRepository.Update(user, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Password reset successfully for user {id}",
                    null), ct);
                return Result<Guid>.Success(user.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"No changes were made when resetting password for user {id}",
                null), ct);
            return Result<Guid>.Failure("Failed to reset password");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while resetting password for user {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Error while resetting password: {e.Message}");
        }
    }

    public async Task<Result<Guid>> ResetPasswordAsAdmin(Guid id, string newPassword, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting admin password reset process for user {id}",
                null), ct);

            var user = await _database.UserRepository.GetByIdAsync(id, ct);
            if (user == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to reset password: user with ID {id} not found",
                    null), ct);
                return Result<Guid>.Failure("User not found");
            }

            user.PasswordHash = _hasher.ComputeHash(newPassword);

            _database.UserRepository.Update(user, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Password reset successfully by admin for user {id}",
                    null), ct);
                return Result<Guid>.Success(user.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"No changes were made when resetting password by admin for user {id}",
                null), ct);
            return Result<Guid>.Failure("Failed to reset password");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while resetting password by admin for user {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Error while resetting password: {e.Message}");
        }
    }

    public async Task<Result<int>> GetUserCount(CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                "Starting to count users",
                null), ct);

            var usersCount = await _database.UserRepository.CountAsync(ct);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Counted {usersCount} users",
                null), ct);

            return Result<int>.Success(usersCount);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while counting users: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<int>.Failure($"Error while counting users: {e.Message}");
        }
    }

    public async Task<Result<UserProfileDTO>> GetUserProfile(Guid id, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to retrieve profile for user {id}",
                null), ct);

            var user = await _database.UserRepository.GetByIdAsync(id, ct);
            if (user == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"User with ID {id} not found",
                    null), ct);
                return Result<UserProfileDTO>.Failure("User not found");
            }

            var dto = new UserProfileDTO(
                Id: user.Id,
                Fullname: user.FullName,
                Email: user.Email,
                Username: user.Username,
                Birthday: user.Birthday.ToString(CultureInfo.CurrentCulture),
                HiredAt: user.HiredAt.ToString(CultureInfo.CurrentCulture),
                FiredAt: user.FiredAt?.ToString(CultureInfo.CurrentCulture) ?? string.Empty,
                IsMale: user.IsMale,
                IsAdmin: user.IsAdmin,
                HasPublishedRights: user.HasPublishedRights,
                DepartmentName: user.Department?.Name ?? string.Empty,
                PostsAmount: user.Posts.Count,
                CommentsAmount: user.Comments.Count,
                MessagesAmount: user.Messages.Count
            );
            
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Successfully retrieved profile for user {id}",
                null), ct);

            return Result<UserProfileDTO>.Success(dto);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while getting profile for user {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<UserProfileDTO>.Failure($"Error while getting info: {e.Message}");
        }
    }
}