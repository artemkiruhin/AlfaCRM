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

    public async
        Task<Result<(Guid id, string username, Guid departmentId, bool isSpecDepartment, bool isAdmin, string token)>>
        Login(LoginRequest request, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало процесса входа для пользователя: {request.Username}",
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
                    $"Ошибка входа для пользователя: {request.Username} - пользователь не найден или неверные учетные данные",
                    null), ct);
                return Result<(Guid id, string username, Guid departmentId, bool isSpecDepartment, bool isAdmin, string
                    token)>.Failure("Неверное имя пользователя или пароль");
            }

            if (user.IsBlocked)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Вход заблокирован для пользователя {user.Id} (имя: {user.Username}) - учетная запись заблокирована",
                    user.Id), ct);
                return Result<(Guid id, string username, Guid departmentId, bool isSpecDepartment, bool isAdmin, string
                    token)>.Failure("Учетная запись заблокирована");
            }

            var token = _jwtService.GenerateToken(user.Id);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Пользователь {user.Id} (имя: {user.Username}) успешно вошел в систему",
                user.Id), ct);

            return Result<(Guid id, string username, Guid departmentId, bool isSpecDepartment, bool isAdmin, string
                    token)>
                .Success((user.Id, user.Username, user.DepartmentId ?? Guid.Empty, user.Department?.IsSpecific ?? false,
                    user.IsAdmin, token));
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при входе для пользователя {request.Username}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<(Guid id, string username, Guid departmentId, bool isSpecDepartment, bool isAdmin, string
                token)>.Failure($"Ошибка при входе: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Create(UserCreateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало процесса создания пользователя. Запрос: {System.Text.Json.JsonSerializer.Serialize(request)}",
                null), ct);

            var existingUser = await _database.UserRepository.GetByUsernameAsync(request.Username, ct);
            if (existingUser != null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Ошибка создания пользователя: имя {request.Username} уже существует",
                    null), ct);
                return Result<Guid>.Failure("Имя пользователя уже существует");
            }

            var department = await _database.DepartmentRepository.GetByIdAsync(request.DepartmentId, ct);
            if (department == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Ошибка создания пользователя: отдел с ID {request.DepartmentId} не найден",
                    null), ct);
                return Result<Guid>.Failure("Отдел не найден");
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
                    $"Пользователь успешно создан с ID: {newUser.Id}",
                    newUser.Id), ct);
                return Result<Guid>.Success(newUser.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                "Не было изменений при создании пользователя",
                null), ct);
            return Result<Guid>.Failure("Ошибка при создании пользователя");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при создании пользователя: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Ошибка при создании пользователя: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Update(UserUpdateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало процесса обновления пользователя с ID {request.Id}. Запрос: {System.Text.Json.JsonSerializer.Serialize(request)}",
                null), ct);

            var user = await _database.UserRepository.GetByIdAsync(request.Id, ct);
            if (user == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Ошибка обновления пользователя: пользователь с ID {request.Id} не найден",
                    null), ct);
                return Result<Guid>.Failure("Пользователь не найден");
            }

            if (!string.IsNullOrEmpty(request.Email))
            {
                var userByEmail =
                    await _database.UserRepository.FindAsync(u => u.Email == request.Email && u.Id != request.Id, ct);
                if (userByEmail != null)
                {
                    await _database.LogRepository.CreateAsync(LogEntity.Create(
                        LogType.Warning,
                        $"Ошибка обновления пользователя: email {request.Email} уже существует",
                        null), ct);
                    return Result<Guid>.Failure("Email уже существует");
                }

                user.Email = request.Email;
            }

            if (request.IsAdmin.HasValue)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Обновление IsAdmin с {user.IsAdmin} на {request.IsAdmin.Value} для пользователя {request.Id}",
                    null), ct);
                user.IsAdmin = request.IsAdmin.Value;
            }

            if (request.HasPublishedRights.HasValue)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Обновление HasPublishedRights с {user.HasPublishedRights} на {request.HasPublishedRights.Value} для пользователя {request.Id}",
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
                        $"Ошибка обновления пользователя: отдел с ID {request.DepartmentId} не найден",
                        null), ct);
                    return Result<Guid>.Failure("Отдел не найден");
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
                    $"Пользователь {request.Id} успешно обновлен",
                    request.Id), ct);
                return Result<Guid>.Success(user.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"Не было изменений при обновлении пользователя {request.Id}",
                null), ct);
            return Result<Guid>.Failure("Ошибка при обновлении пользователя");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при обновлении пользователя {request.Id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Ошибка при обновлении пользователя: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало процесса удаления пользователя с ID {id}",
                null), ct);

            var user = await _database.UserRepository.GetByIdAsync(id, ct);
            if (user == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Ошибка удаления пользователя: пользователь с ID {id} не найден",
                    null), ct);
                return Result<Guid>.Failure("Пользователь не найден");
            }

            // Логирование связанных сущностей, которые будут затронуты
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Удаление пользователя {id} с {user.Posts.Count} постами, {user.Comments.Count} комментариями и {user.Messages.Count} сообщениями",
                null), ct);

            _database.UserRepository.Delete(user, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Пользователь {id} успешно удален",
                    null), ct);
                return Result<Guid>.Success(user.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"Не было изменений при удалении пользователя {id}",
                null), ct);
            return Result<Guid>.Failure("Ошибка при удалении пользователя");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при удалении пользователя {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Ошибка при удалении пользователя: {e.Message}");
        }
    }

    public async Task<Result<List<UserShortDTO>>> GetAllShort(CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                "Начало получения списка пользователей (краткая версия)",
                null), ct);

            var users = await _database.UserRepository.GetAllAsync(ct);
            var dtos = MapToShortDTORange(users);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Получено {dtos.Count} пользователей (краткая версия)",
                null), ct);

            return Result<List<UserShortDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении пользователей (краткая версия): {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<List<UserShortDTO>>.Failure($"Ошибка при получении пользователей: {e.Message}");
        }
    }

    public async Task<Result<List<UserDetailedDTO>>> GetAll(CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                "Начало получения списка пользователей (подробная версия)",
                null), ct);

            var users = await _database.UserRepository.GetAllAsync(ct);
            var dtos = users.Select(MapToDetailedDTO).ToList();

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Получено {dtos.Count} пользователей (подробная версия)",
                null), ct);

            return Result<List<UserDetailedDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении пользователей (подробная версия): {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<List<UserDetailedDTO>>.Failure($"Ошибка при получении пользователей: {e.Message}");
        }
    }

    public async Task<Result<UserDetailedDTO>> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало получения пользователя (подробно) с ID {id}",
                null), ct);

            var user = await _database.UserRepository.GetByIdAsync(id, ct);
            if (user == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Пользователь с ID {id} не найден",
                    null), ct);
                return Result<UserDetailedDTO>.Failure("Пользователь не найден");
            }

            var dto = MapToDetailedDTO(user);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Пользователь (подробно) с ID {id} успешно получен",
                null), ct);

            return Result<UserDetailedDTO>.Success(dto);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении пользователя (подробно) {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<UserDetailedDTO>.Failure($"Ошибка при получении пользователя: {e.Message}");
        }
    }

    public async Task<Result<UserShortDTO>> GetByIdShort(Guid id, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало получения пользователя (кратко) с ID {id}",
                null), ct);

            var user = await _database.UserRepository.GetByIdAsync(id, ct);
            if (user == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Пользователь с ID {id} не найден",
                    null), ct);
                return Result<UserShortDTO>.Failure("Пользователь не найден");
            }

            var dto = MapToShortDTO(user);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Пользователь (кратко) с ID {id} успешно получен",
                null), ct);

            return Result<UserShortDTO>.Success(dto);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении пользователя (кратко) {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<UserShortDTO>.Failure($"Ошибка при получении пользователя: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Block(Guid id, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало блокировки пользователя с ID {id}",
                null), ct);

            var user = await _database.UserRepository.GetByIdAsync(id, ct);
            if (user == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Ошибка блокировки пользователя: пользователь с ID {id} не найден",
                    null), ct);
                return Result<Guid>.Failure("Пользователь не найден");
            }

            if (user.IsBlocked)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Пользователь {id} уже заблокирован",
                    null), ct);
                return Result<Guid>.Failure("Пользователь уже заблокирован");
            }

            user.IsBlocked = true;
            user.IsActive = false;

            _database.UserRepository.Update(user, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Пользователь {id} успешно заблокирован",
                    null), ct);
                return Result<Guid>.Success(user.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"Не было изменений при блокировке пользователя {id}",
                null), ct);
            return Result<Guid>.Failure("Ошибка при блокировке пользователя");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при блокировке пользователя {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Ошибка при блокировке пользователя: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Fire(Guid id, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало увольнения пользователя с ID {id}",
                null), ct);

            var user = await _database.UserRepository.GetByIdAsync(id, ct);
            if (user == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Ошибка увольнения пользователя: пользователь с ID {id} не найден",
                    null), ct);
                return Result<Guid>.Failure("Пользователь не найден");
            }

            if (user.FiredAt.HasValue)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Пользователь {id} уже уволен",
                    null), ct);
                return Result<Guid>.Failure("Пользователь уже уволен");
            }

            user.IsBlocked = true;
            user.IsActive = false;
            user.FiredAt = DateTime.UtcNow;

            _database.UserRepository.Update(user, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            Console.WriteLine($"{result}");

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Пользователь {id} успешно уволен",
                    null), ct);
                return Result<Guid>.Success(user.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"Не было изменений при увольнении пользователя {id}",
                null), ct);
            return Result<Guid>.Failure("Ошибка при увольнении пользователя");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при увольнении пользователя {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Ошибка при увольнении пользователя: {e.Message}");
        }
    }

    public async Task<Result<Guid>> ResetPassword(Guid id, string oldPassword, string newPassword, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало сброса пароля для пользователя {id}",
                null), ct);

            if (oldPassword == newPassword)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    "Новый пароль должен отличаться от старого",
                    null), ct);
                return Result<Guid>.Failure("Новый пароль должен отличаться от старого");
            }

            var user = await _database.UserRepository.GetByIdAsync(id, ct);
            if (user == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Ошибка сброса пароля: пользователь с ID {id} не найден",
                    null), ct);
                return Result<Guid>.Failure("Пользователь не найден");
            }

            var oldPasswordHashed = _hasher.ComputeHash(oldPassword);
            var newPasswordHashed = _hasher.ComputeHash(newPassword);

            if (user.PasswordHash != oldPasswordHashed)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    "Старый пароль неверный",
                    null), ct);
                return Result<Guid>.Failure("Старый пароль неверный");
            }

            user.PasswordHash = newPasswordHashed;

            _database.UserRepository.Update(user, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Пароль успешно сброшен для пользователя {id}",
                    null), ct);
                return Result<Guid>.Success(user.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"Не было изменений при сбросе пароля для пользователя {id}",
                null), ct);
            return Result<Guid>.Failure("Ошибка при сбросе пароля");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при сбросе пароля для пользователя {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Ошибка при сбросе пароля: {e.Message}");
        }
    }

    public async Task<Result<Guid>> ResetPasswordAsAdmin(Guid id, string newPassword, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало сброса пароля администратором для пользователя {id}",
                null), ct);

            var user = await _database.UserRepository.GetByIdAsync(id, ct);
            if (user == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Ошибка сброса пароля: пользователь с ID {id} не найден",
                    null), ct);
                return Result<Guid>.Failure("Пользователь не найден");
            }

            user.PasswordHash = _hasher.ComputeHash(newPassword);

            _database.UserRepository.Update(user, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Пароль успешно сброшен администратором для пользователя {id}",
                    null), ct);
                return Result<Guid>.Success(user.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"Не было изменений при сбросе пароля администратором для пользователя {id}",
                null), ct);
            return Result<Guid>.Failure("Ошибка при сбросе пароля");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при сбросе пароля администратором для пользователя {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Ошибка при сбросе пароля: {e.Message}");
        }
    }

    public async Task<Result<int>> GetUserCount(CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                "Начало подсчета пользователей",
                null), ct);

            var usersCount = await _database.UserRepository.CountAsync(ct);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Подсчитано {usersCount} пользователей",
                null), ct);

            return Result<int>.Success(usersCount);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при подсчете пользователей: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<int>.Failure($"Ошибка при подсчете пользователей: {e.Message}");
        }
    }

    public async Task<Result<UserProfileDTO>> GetUserProfile(Guid id, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало получения профиля пользователя {id}",
                null), ct);

            var user = await _database.UserRepository.GetByIdAsync(id, ct);
            if (user == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Пользователь с ID {id} не найден",
                    null), ct);
                return Result<UserProfileDTO>.Failure("Пользователь не найден");
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
                $"Профиль пользователя {id} успешно получен",
                null), ct);

            return Result<UserProfileDTO>.Success(dto);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении профиля пользователя {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<UserProfileDTO>.Failure($"Ошибка при получении информации: {e.Message}");
        }
    }
}