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
                $"Начало процесса создания отдела. Запрос: {System.Text.Json.JsonSerializer.Serialize(request)}",
                null), ct);

            var dbDepartment = await _database.DepartmentRepository.GetDepartmentByName(request.Name, ct);
            if (dbDepartment != null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось создать отдел: отдел с именем '{request.Name}' уже существует",
                    null), ct);
                return Result<Guid>.Failure($"Отдел {request.Name} уже существует");
            }

            var newDepartment = DepartmentEntity.Create(request.Name, request.IsSpecific);
            await _database.DepartmentRepository.CreateAsync(newDepartment, ct);
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Отдел успешно создан с ID: {newDepartment.Id}",
                null), ct);

            var department = await _database.DepartmentRepository.GetByIdAsync(newDepartment.Id, ct);
            if (department == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Error,
                    $"Отдел с ID {newDepartment.Id} не найден после создания",
                    null), ct);
                return Result<Guid>.Failure("Отдел не найден");
            }

            return Result<Guid>.Success(department.Id);
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при создании отдела: {ex.Message}. StackTrace: {ex.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Ошибка при создании отдела: {ex.Message}");
        }
    }

    public async Task<Result<Guid>> Update(DepartmentUpdateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало процесса обновления отдела с ID {request.DepartmentId}. Запрос: {System.Text.Json.JsonSerializer.Serialize(request)}",
                null), ct);

            if (!request.IsSpecific.HasValue && string.IsNullOrEmpty(request.Name))
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    "Не удалось обновить отдел: необходимо указать хотя бы одно поле для обновления",
                    null), ct);
                return Result<Guid>.Failure("Необходимо указать хотя бы одно поле для обновления");
            }

            var dbDepartment = await _database.DepartmentRepository.GetByIdAsync(request.DepartmentId, ct);
            if (dbDepartment == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось обновить отдел: отдел с ID {request.DepartmentId} не найден",
                    null), ct);
                return Result<Guid>.Failure("Отдел не найден");
            }

            if (!string.IsNullOrEmpty(request.Name) && dbDepartment.Name != request.Name)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Обновление названия отдела {request.DepartmentId} с '{dbDepartment.Name}' на '{request.Name}'",
                    null), ct);
                dbDepartment.Name = request.Name;
            }

            if (request.IsSpecific.HasValue && request.IsSpecific != dbDepartment.IsSpecific)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Обновление свойства IsSpecific отдела {request.DepartmentId} с {dbDepartment.IsSpecific} на {request.IsSpecific.Value}",
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
                    $"Отдел {request.DepartmentId} успешно обновлен",
                    null), ct);
                return Result<Guid>.Success(dbDepartment.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"Не было внесено изменений в отдел {request.DepartmentId}",
                null), ct);
            return Result<Guid>.Failure("Не удалось обновить отдел");
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при обновлении отдела {request?.DepartmentId}: {ex.Message}. StackTrace: {ex.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Ошибка при обновлении отдела: {ex.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало процесса удаления отдела с ID {id}",
                null), ct);

            var dbDepartment = await _database.DepartmentRepository.GetByIdAsync(id, ct);
            if (dbDepartment == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось удалить отдел: отдел с ID {id} не найден",
                    null), ct);
                return Result<Guid>.Failure("Отдел не найден");
            }

            _database.DepartmentRepository.Delete(dbDepartment, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Отдел {id} успешно удален",
                    null), ct);
                return Result<Guid>.Success(dbDepartment.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"Не было внесено изменений при попытке удаления отдела {id}",
                null), ct);
            return Result<Guid>.Failure("Не удалось удалить отдел");
        }
        catch (Exception ex)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при удалении отдела {id}: {ex.Message}. StackTrace: {ex.StackTrace}",
                null), ct);
            return Result<Guid>.Failure($"Ошибка при удалении отдела: {ex.Message}");
        }
    }

    public async Task<Result<List<DepartmentShortDTO>>> GetAllShort(CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                "Начало получения списка всех отделов (краткая версия)",
                null), ct);

            var entities = await _database.DepartmentRepository.GetAllAsync(ct);
            var dtos = MapShortRange(entities);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Получено {dtos.Count} отделов (краткая версия)",
                null), ct);

            return Result<List<DepartmentShortDTO>>.Success(dtos);
        }
        catch (Exception ex)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении списка всех отделов (краткая версия): {ex.Message}. StackTrace: {ex.StackTrace}",
                null), ct);
            return Result<List<DepartmentShortDTO>>.Failure($"Ошибка при получении отделов: {ex.Message}");
        }
    }

    public async Task<Result<List<DepartmentDetailedDTO>>> GetAll(CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                "Начало получения списка всех отделов (подробная версия)",
                null), ct);

            var entities = await _database.DepartmentRepository.GetAllAsync(ct);
            var dtos = MapDetailedRange(entities);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Получено {dtos.Count} отделов (подробная версия)",
                null), ct);

            return Result<List<DepartmentDetailedDTO>>.Success(dtos);
        }
        catch (Exception ex)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении списка всех отделов (подробная версия): {ex.Message}. StackTrace: {ex.StackTrace}",
                null), ct);
            return Result<List<DepartmentDetailedDTO>>.Failure($"Ошибка при получении отделов: {ex.Message}");
        }
    }

    public async Task<Result<DepartmentDetailedDTO>> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало получения отдела (подробная версия) с ID {id}",
                null), ct);

            var entity = await _database.DepartmentRepository.GetByIdAsync(id, ct);
            if (entity == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Отдел с ID {id} не найден",
                    null), ct);
                return Result<DepartmentDetailedDTO>.Failure("Отдел не найден");
            }

            var dto = MapDetailed(entity);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Отдел (подробная версия) с ID {id} успешно получен",
                null), ct);

            return Result<DepartmentDetailedDTO>.Success(dto);
        }
        catch (Exception ex)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении отдела (подробная версия) с ID {id}: {ex.Message}. StackTrace: {ex.StackTrace}",
                null), ct);
            return Result<DepartmentDetailedDTO>.Failure($"Ошибка при получении отдела: {ex.Message}");
        }
    }

    public async Task<Result<DepartmentShortDTO>> GetByIdShort(Guid id, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало получения отдела (краткая версия) с ID {id}",
                null), ct);

            var entity = await _database.DepartmentRepository.GetByIdAsync(id, ct);
            if (entity == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Отдел с ID {id} не найден",
                    null), ct);
                return Result<DepartmentShortDTO>.Failure("Отдел не найден");
            }

            var dto = MapShort(entity);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Отдел (краткая версия) с ID {id} успешно получен",
                null), ct);

            return Result<DepartmentShortDTO>.Success(dto);
        }
        catch (Exception ex)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении отдела (краткая версия) с ID {id}: {ex.Message}. StackTrace: {ex.StackTrace}",
                null), ct);
            return Result<DepartmentShortDTO>.Failure($"Ошибка при получении отдела: {ex.Message}");
        }
    }

    public async Task<Result<int>> GetDepartmentCount(CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                "Начало подсчета количества отделов",
                null), ct);

            var departmentCount = await _database.DepartmentRepository.CountAsync(ct);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Подсчет отделов завершен: найдено {departmentCount} отделов",
                null), ct);

            return Result<int>.Success(departmentCount);
        }
        catch (Exception ex)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при подсчете количества отделов: {ex.Message}. StackTrace: {ex.StackTrace}",
                null), ct);
            return Result<int>.Failure($"Ошибка при подсчете отделов: {ex.Message}");
        }
    }
}