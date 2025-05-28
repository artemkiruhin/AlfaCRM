using AlfaCRM.Domain.Interfaces.Database;
using AlfaCRM.Domain.Interfaces.Services.Entity;
using AlfaCRM.Domain.Models.Contracts;
using AlfaCRM.Domain.Models.DTOs;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Services.Entity;

public class TicketService : ITicketService
{
    private readonly IUnitOfWork _database;

    public TicketService(IUnitOfWork database)
    {
        _database = database;
    }

    public async Task<Result<Guid>> Create(TicketCreateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало процесса создания тикета. Запрос: {System.Text.Json.JsonSerializer.Serialize(request)}",
                request.CreatorId), ct);

            var creator = await _database.UserRepository.GetByIdAsync(request.CreatorId, ct);
            if (creator == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось создать тикет: создатель с ID {request.CreatorId} не найден",
                    request.CreatorId), ct);
                return Result<Guid>.Failure("Создатель не найден");
            }
            
            var departmentDb = await _database.DepartmentRepository.GetByIdAsync(request.DepartmentId, ct);
            if (departmentDb == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось создать тикет: отдел с ID {request.DepartmentId} не найден",
                    request.CreatorId), ct);
                return Result<Guid>.Failure("Отдел не найден");
            }

            var ticket = TicketEntity.Create(
                request.Title,
                request.Text,
                request.DepartmentId,
                TicketStatus.Created, 
                null,
                null,
                request.CreatorId,
                request.Type);

        await _database.TicketRepository.CreateAsync(ticket, ct);
        
        try
        {
            var department = await _database.DepartmentRepository.GetByIdAsync(request.DepartmentId, ct);
            if (department != null)
            {
                var usersInDepartment = (await _database.UserRepository.FindRangeAsync(user => user.DepartmentId == department.Id, ct))
                    .Where(u => !u.IsBlocked)
                    .ToList();

                if (usersInDepartment.Any())
                {
                    var assigneeLoads = new List<(UserEntity User, int Load)>();
                    
                    foreach (var user in usersInDepartment)
                    {
                        var load = await _database.TicketRepository.CountAsync(t => 
                            t.AssigneeId == user.Id && t.Status == TicketStatus.InWork, ct);
                        assigneeLoads.Add((user, load));
                    }

                    var minLoad = assigneeLoads.Min(al => al.Load);
                    var candidates = assigneeLoads.Where(al => al.Load == minLoad).ToList();

                    if (candidates.Count != 0)
                    {
                        var selected = candidates.First();
                        ticket.AssigneeId = selected.User.Id;
                        ticket.Status = TicketStatus.InWork;

                        await _database.LogRepository.CreateAsync(LogEntity.Create(
                            LogType.Info,
                            $"Тикет {ticket.Id} автоматически назначен на сотрудника {selected.User.Username} (ID: {selected.User.Id}) с нагрузкой {selected.Load}",
                            request.CreatorId), ct);
                    }
                }
                else
                {
                    await _database.LogRepository.CreateAsync(LogEntity.Create(
                        LogType.Warning,
                        $"В отделе {department.Name} (ID: {department.Id}) нет активных сотрудников",
                        request.CreatorId), ct);
                }
            }
        }
        catch (Exception ex)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при автоматическом назначении: {ex.Message}",
                request.CreatorId), ct);
        }

        var result = await _database.SaveChangesAsync(ct);
        await _database.CommitTransactionAsync(ct);


            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Тикет успешно создан с ID: {ticket.Id}",
                    request.CreatorId), ct);
                return Result<Guid>.Success(ticket.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                "Не было внесено изменений при создании тикета",
                request.CreatorId), ct);
            return Result<Guid>.Failure("Не удалось создать тикет");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при создании тикета: {e.Message}. StackTrace: {e.StackTrace}",
                request?.CreatorId), ct);
            return Result<Guid>.Failure($"Ошибка при создании тикета: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Update(TicketUpdateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало обновления тикета с ID {request.Id}. Запрос: {System.Text.Json.JsonSerializer.Serialize(request)}",
                request.SenderId), ct);

            var ticket = await _database.TicketRepository.GetByIdAsync(request.Id, ct);
            if (ticket == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось обновить тикет: тикет с ID {request.Id} не найден",
                    request.SenderId), ct);
                return Result<Guid>.Failure($"Тикет не найден: {request.Id}");
            }

            var sender = await _database.UserRepository.GetByIdAsync(request.SenderId, ct);
            if (sender == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось обновить тикет: отправитель с ID {request.SenderId} не найден",
                    request.SenderId), ct);
                return Result<Guid>.Failure($"Отправитель не найден: {request.SenderId}");
            }

            if (sender is { IsAdmin: false, DepartmentId: null })
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось обновить тикет: отправитель {request.SenderId} не имеет прав администратора",
                    request.SenderId), ct);
                return Result<Guid>.Failure("Отправитель не имеет прав администратора");
            }

            // Если отправитель - создатель
            if (ticket.CreatorId == request.SenderId && ticket.Status == TicketStatus.Created)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Обновление тикета {request.Id} как создатель",
                    request.SenderId), ct);

                if (!string.IsNullOrEmpty(request.Title)) ticket.Title = request.Title;
                if (!string.IsNullOrEmpty(request.Text)) ticket.Text = request.Text;
                if (request.DepartmentId.HasValue) ticket.DepartmentId = request.DepartmentId.Value;
            }
            
            if (sender.DepartmentId.HasValue && ticket.DepartmentId == sender.DepartmentId)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Обновление тикета {request.Id} как эксперт отдела",
                    request.SenderId), ct);

                if (ticket.Status is TicketStatus.Completed or TicketStatus.Rejected)
                {
                    if (!string.IsNullOrEmpty(request.Feedback)) ticket.Feedback = request.Feedback;
                }
            }

            if (sender.IsAdmin)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Обновление тикета {request.Id} как администратор",
                    request.SenderId), ct);

                if (!string.IsNullOrEmpty(request.Title)) ticket.Title = request.Title;
                if (!string.IsNullOrEmpty(request.Text)) ticket.Text = request.Text;
                if (request.DepartmentId.HasValue) ticket.DepartmentId = request.DepartmentId.Value;
                if (!string.IsNullOrEmpty(request.Feedback)) ticket.Feedback = request.Feedback;
                if (request.Type.HasValue) ticket.Type = request.Type.Value;
            }

            _database.TicketRepository.Update(ticket, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Тикет {request.Id} успешно обновлен",
                    request.SenderId), ct);
                return Result<Guid>.Success(ticket.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"Не было внесено изменений при обновлении тикета {request.Id}",
                request.SenderId), ct);
            return Result<Guid>.Failure("Не удалось обновить тикет");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при обновлении тикета {request?.Id}: {e.Message}. StackTrace: {e.StackTrace}",
                request?.SenderId), ct);
            return Result<Guid>.Failure($"Ошибка при обновлении тикета: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id, Guid userId, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало удаления тикета с ID {id} пользователем {userId}",
                userId), ct);

            var ticket = await _database.TicketRepository.GetByIdAsync(id, ct);
            if (ticket == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось удалить тикет: тикет с ID {id} не найден",
                    userId), ct);
                return Result<Guid>.Failure($"Тикет не найден: {id}");
            }

            var user = await _database.UserRepository.GetByIdAsync(userId, ct);
            if (user == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось удалить тикет: пользователь с ID {userId} не найден",
                    userId), ct);
                return Result<Guid>.Failure($"Пользователь не найден: {userId}");
            }

            if (!user.IsAdmin)
            {
                if (ticket.CreatorId != userId)
                {
                    await _database.LogRepository.CreateAsync(LogEntity.Create(
                        LogType.Warning,
                        $"Пользователь {userId} не является администратором или создателем тикета",
                        userId), ct);
                    return Result<Guid>.Failure("Только администратор или создатель может удалить тикет");
                }

                if (ticket.Status != TicketStatus.Created)
                {
                    await _database.LogRepository.CreateAsync(LogEntity.Create(
                        LogType.Warning,
                        $"Тикет {id} не в статусе 'Создано' (текущий статус: {ticket.Status})",
                        userId), ct);
                    return Result<Guid>.Failure("Создатель может удалять только тикеты в статусе 'Создано'");
                }
            }

            _database.TicketRepository.Delete(ticket, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Тикет {id} успешно удален пользователем {userId}",
                    userId), ct);
                return Result<Guid>.Success(ticket.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"Не было внесено изменений при удалении тикета {id}",
                userId), ct);
            return Result<Guid>.Failure("Не удалось удалить тикет");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при удалении тикета {id}: {e.Message}. StackTrace: {e.StackTrace}",
                userId), ct);
            return Result<Guid>.Failure($"Ошибка при удалении тикета: {e.Message}");
        }
    }

    public async Task<Result<List<TicketShortDTO>>> GetAllShort(Guid? departmentId, Guid? senderId, TicketType? type,
        CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало получения краткой информации о тикетах (Отдел: {departmentId}, Отправитель: {senderId}, Тип: {type})",
                null), ct);

            var tickets = await _database.TicketRepository.GetAllAsync(ct);

            if (departmentId.HasValue) tickets = tickets.Where(ticket => ticket.DepartmentId == departmentId);
            if (senderId.HasValue) tickets = tickets.Where(ticket => ticket.CreatorId == senderId);
            if (type.HasValue) tickets = tickets.Where(ticket => ticket.Type == type);

            var dtos = tickets.Select(ticket => new TicketShortDTO(
                Id: ticket.Id,
                Title: ticket.Title,
                Feedback: ticket.Feedback,
                DepartmentId: ticket.DepartmentId,
                DepartmentName: ticket.Department?.Name ?? "Без отдела",
                CreatedAt: ticket.CreatedAt,
                Status: ticket.Status switch
                {
                    TicketStatus.Created => "Создано",
                    TicketStatus.InWork => "В работе",
                    TicketStatus.Completed => "Выполнено",
                    TicketStatus.Rejected => "Отменено",
                    _ => throw new ArgumentOutOfRangeException(nameof(ticket.Status))
                },
                AssigneeId: ticket.AssigneeId,
                AssigneeUsername: ticket.Assignee?.Username ?? string.Empty,
                ClosedAt: ticket.ClosedAt,
                CreatorId: ticket.CreatorId,
                CreatorUsername: ticket.Creator?.Username ?? string.Empty,
                Type: ticket.Type switch
                {
                    TicketType.ProblemCase => "Заявка",
                    TicketType.Suggestion => "Предложение",
                    _ => throw new ArgumentOutOfRangeException(nameof(ticket.Type))
                }
            )).ToList();

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Получено {dtos.Count} кратких описаний тикетов",
                null), ct);

            return Result<List<TicketShortDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении краткой информации о тикетах: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<List<TicketShortDTO>>.Failure($"Ошибка при получении тикетов: {e.Message}");
        }
    }

    public async Task<Result<List<TicketDetailedDTO>>> GetAll(Guid? departmentId, Guid? senderId, TicketType? type,
        CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало получения подробной информации о тикетах (Отдел: {departmentId}, Отправитель: {senderId}, Тип: {type})",
                null), ct);

            var tickets = await _database.TicketRepository.GetAllAsync(ct);

            if (departmentId.HasValue) tickets = tickets.Where(ticket => ticket.DepartmentId == departmentId);
            if (senderId.HasValue) tickets = tickets.Where(ticket => ticket.CreatorId == senderId);
            if (type.HasValue) tickets = tickets.Where(ticket => ticket.Type == type);

            var dtos = tickets.Select(ticket => new TicketDetailedDTO(
                Id: ticket.Id,
                Title: ticket.Title,
                Text: ticket.Text,
                Feedback: ticket.Feedback,
                Department: ticket.DepartmentId != null
                    ? new DepartmentShortDTO(
                        Id: ticket.DepartmentId,
                        Name: ticket.Department?.Name ?? string.Empty,
                        MembersCount: ticket.Department?.Users.Count ?? 0,
                        IsSpecific: ticket.Department?.IsSpecific ?? false
                    )
                    : null,
                CreatedAt: ticket.CreatedAt,
                Status: ticket.Status switch
                {
                    TicketStatus.Created => "Создано",
                    TicketStatus.InWork => "В работе",
                    TicketStatus.Completed => "Выполнено",
                    TicketStatus.Rejected => "Отменено",
                    _ => throw new ArgumentOutOfRangeException(nameof(ticket.Status))
                },
                Assignee: ticket.Assignee == null
                    ? null
                    : new UserShortDTO(
                        Id: ticket.Assignee.Id,
                        FullName: ticket.Assignee.FullName,
                        Username: ticket.Assignee.Username,
                        Email: ticket.Assignee.Email,
                        DepartmentName: ticket.Assignee?.Department?.Name ?? "Без отдела",
                        IsAdmin: ticket.Assignee?.IsAdmin ?? false,
                        IsBlocked: ticket.Assignee?.IsBlocked ?? false
                    ),
                Creator: new UserShortDTO(
                    Id: ticket.Creator.Id,
                    FullName: ticket.Creator.FullName,
                    Username: ticket.Creator.Username,
                    Email: ticket.Creator.Email,
                    DepartmentName: ticket.Creator?.Department?.Name ?? "Без отдела",
                    IsAdmin: ticket.Creator?.IsAdmin ?? false,
                    IsBlocked: ticket.Creator?.IsBlocked ?? false
                ),
                ClosedAt: ticket.ClosedAt,
                Type: ticket.Type switch
                {
                    TicketType.ProblemCase => "Заявка",
                    TicketType.Suggestion => "Предложение",
                    _ => throw new ArgumentOutOfRangeException(nameof(ticket.Type))
                }
            )).ToList();

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Получено {dtos.Count} подробных описаний тикетов",
                null), ct);

            return Result<List<TicketDetailedDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении подробной информации о тикетах: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<List<TicketDetailedDTO>>.Failure($"Ошибка при получении тикетов: {e.Message}");
        }
    }

    public async Task<Result<TicketDetailedDTO>> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало получения тикета с ID {id}",
                null), ct);

            var ticket = await _database.TicketRepository.GetByIdAsync(id, ct);
            if (ticket == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Тикет с ID {id} не найден",
                    null), ct);
                return Result<TicketDetailedDTO>.Failure($"Тикет не найден: {id}");
            }

            var dto = new TicketDetailedDTO(
                Id: ticket.Id,
                Title: ticket.Title,
                Text: ticket.Text,
                Feedback: ticket.Feedback,
                Department: ticket.DepartmentId != null
                    ? new DepartmentShortDTO(
                        Id: ticket.DepartmentId,
                        Name: ticket.Department?.Name ?? string.Empty,
                        MembersCount: ticket.Department?.Users.Count ?? 0,
                        IsSpecific: ticket.Department?.IsSpecific ?? false
                    )
                    : null,
                CreatedAt: ticket.CreatedAt,
                Status: ticket.Status switch
                {
                    TicketStatus.Created => "Создано",
                    TicketStatus.InWork => "В работе",
                    TicketStatus.Completed => "Выполнено",
                    TicketStatus.Rejected => "Отменено",
                    _ => throw new ArgumentOutOfRangeException(nameof(ticket.Status))
                },
                Assignee: ticket.Assignee == null
                    ? null
                    : new UserShortDTO(
                        Id: ticket.Assignee.Id,
                        FullName: ticket.Assignee.FullName,
                        Username: ticket.Assignee.Username,
                        Email: ticket.Assignee.Email,
                        DepartmentName: ticket.Assignee?.Department?.Name ?? "Без отдела",
                        IsAdmin: ticket.Assignee?.IsAdmin ?? false,
                        IsBlocked: ticket.Assignee?.IsBlocked ?? false
                    ),
                Creator: new UserShortDTO(
                    Id: ticket.Creator.Id,
                    FullName: ticket.Creator.FullName,
                    Username: ticket.Creator.Username,
                    Email: ticket.Creator.Email,
                    DepartmentName: ticket.Creator?.Department?.Name ?? "Без отдела",
                    IsAdmin: ticket.Creator?.IsAdmin ?? false,
                    IsBlocked: ticket.Creator?.IsBlocked ?? false
                ),
                ClosedAt: ticket.ClosedAt,
                Type: ticket.Type switch
                {
                    TicketType.ProblemCase => "Заявка",
                    TicketType.Suggestion => "Предложение",
                    _ => throw new ArgumentOutOfRangeException(nameof(ticket.Type))
                }
            );

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Тикет с ID {id} успешно получен",
                null), ct);

            return Result<TicketDetailedDTO>.Success(dto);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении тикета {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<TicketDetailedDTO>.Failure($"Ошибка при получении тикета: {e.Message}");
        }
    }

    public async Task<Result<TicketShortDTO>> GetByIdShort(Guid id, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало получения краткой информации о тикете с ID {id}",
                null), ct);

            var ticket = await _database.TicketRepository.GetByIdAsync(id, ct);
            if (ticket == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Тикет с ID {id} не найден",
                    null), ct);
                return Result<TicketShortDTO>.Failure($"Тикет не найден: {id}");
            }

            var dto = new TicketShortDTO(
                Id: ticket.Id,
                Title: ticket.Title,
                Feedback: ticket.Feedback,
                DepartmentId: ticket.DepartmentId,
                DepartmentName: ticket.Department?.Name ?? "Без отдела",
                CreatedAt: ticket.CreatedAt,
                Status: ticket.Status switch
                {
                    TicketStatus.Created => "Создано",
                    TicketStatus.InWork => "В работе",
                    TicketStatus.Completed => "Выполнено",
                    TicketStatus.Rejected => "Отменено",
                    _ => throw new ArgumentOutOfRangeException(nameof(ticket.Status))
                },
                AssigneeId: ticket.AssigneeId,
                AssigneeUsername: ticket.Assignee?.Username ?? string.Empty,
                ClosedAt: ticket.ClosedAt,
                CreatorId: ticket.CreatorId,
                CreatorUsername: ticket.Creator?.Username ?? string.Empty,
                Type: ticket.Type switch
                {
                    TicketType.ProblemCase => "Заявка",
                    TicketType.Suggestion => "Предложение",
                    _ => throw new ArgumentOutOfRangeException(nameof(ticket.Type))
                }
            );

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Краткая информация о тикете с ID {id} успешно получена",
                null), ct);

            return Result<TicketShortDTO>.Success(dto);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении краткой информации о тикете {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<TicketShortDTO>.Failure($"Ошибка при получении тикета: {e.Message}");
        }
    }

    public async Task<Result<Guid>> TakeToWork(Guid id, Guid assigneeId, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало взятия тикета {id} в работу исполнителем {assigneeId}",
                assigneeId), ct);

            var ticket = await _database.TicketRepository.GetByIdAsync(id, ct);
            if (ticket == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось взять тикет в работу: тикет с ID {id} не найден",
                    assigneeId), ct);
                return Result<Guid>.Failure($"Тикет не найден: {id}");
            }

            var assignee = await _database.UserRepository.GetByIdAsync(assigneeId, ct);
            if (assignee == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось взять тикет в работу: исполнитель с ID {assigneeId} не найден",
                    assigneeId), ct);
                return Result<Guid>.Failure($"Исполнитель не найден: {assigneeId}");
            }

            if (!assignee.IsAdmin && ((assignee.Department != null && assignee.DepartmentId != ticket.DepartmentId) ||
                                      assignee.Department == null))
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Исполнитель {assigneeId} не является администратором или из другого отдела",
                    assigneeId), ct);
                return Result<Guid>.Failure(
                    $"Исполнитель не является администратором: {assignee.Username} и его отдел: {assignee.Department?.Name}");
            }

            if (ticket.Status == TicketStatus.InWork)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Тикет {id} уже в работе",
                    assigneeId), ct);
                return Result<Guid>.Failure("Тикет уже в работе");
            }

            ticket.Status = TicketStatus.InWork;
            ticket.AssigneeId = assigneeId;

            _database.TicketRepository.Update(ticket, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Тикет {id} успешно взят в работу исполнителем {assigneeId}",
                    assigneeId), ct);
                return Result<Guid>.Success(ticket.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"Не было внесено изменений при взятии тикета {id} в работу",
                assigneeId), ct);
            return Result<Guid>.Failure("Не удалось обновить тикет");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при взятии тикета {id} в работу: {e.Message}. StackTrace: {e.StackTrace}",
                assigneeId), ct);
            return Result<Guid>.Failure($"Ошибка при обновлении тикета: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Complete(Guid id, Guid assigneeId, string feedback, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало завершения тикета {id} исполнителем {assigneeId}",
                assigneeId), ct);

            var ticket = await _database.TicketRepository.GetByIdAsync(id, ct);
            if (ticket == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось завершить тикет: тикет с ID {id} не найден",
                    assigneeId), ct);
                return Result<Guid>.Failure($"Тикет не найден: {id}");
            }

            var assignee = await _database.UserRepository.GetByIdAsync(assigneeId, ct);
            if (assignee == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось завершить тикет: исполнитель с ID {assigneeId} не найден",
                    assigneeId), ct);
                return Result<Guid>.Failure($"Исполнитель не найден: {assigneeId}");
            }

            if (!assignee.IsAdmin && ((assignee.Department != null && assignee.DepartmentId != ticket.DepartmentId) ||
                                      assignee.Department == null))
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Исполнитель {assigneeId} не является администратором или из другого отдела",
                    assigneeId), ct);
                return Result<Guid>.Failure(
                    $"Исполнитель не является администратором: {assignee.Username} и его отдел: {assignee.Department?.Name}");
            }

            if (ticket.Status == TicketStatus.Completed)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Тикет {id} уже завершен",
                    assigneeId), ct);
                return Result<Guid>.Failure("Тикет уже завершен");
            }

            ticket.Status = TicketStatus.Completed;
            ticket.ClosedAt = DateTime.UtcNow;
            ticket.Feedback = feedback;
            ticket.AssigneeId ??= assigneeId;

            _database.TicketRepository.Update(ticket, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Тикет {id} успешно завершен исполнителем {assigneeId}",
                    assigneeId), ct);
                return Result<Guid>.Success(ticket.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"Не было внесено изменений при завершении тикета {id}",
                assigneeId), ct);
            return Result<Guid>.Failure("Не удалось обновить тикет");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при завершении тикета {id}: {e.Message}. StackTrace: {e.StackTrace}",
                assigneeId), ct);
            return Result<Guid>.Failure($"Ошибка при обновлении тикета: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Reject(Guid id, Guid assigneeId, string feedback, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало отклонения тикета {id} исполнителем {assigneeId}",
                assigneeId), ct);

            var ticket = await _database.TicketRepository.GetByIdAsync(id, ct);
            if (ticket == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось отклонить тикет: тикет с ID {id} не найден",
                    assigneeId), ct);
                return Result<Guid>.Failure($"Тикет не найден: {id}");
            }

            var assignee = await _database.UserRepository.GetByIdAsync(assigneeId, ct);
            if (assignee == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Не удалось отклонить тикет: исполнитель с ID {assigneeId} не найден",
                    assigneeId), ct);
                return Result<Guid>.Failure($"Исполнитель не найден: {assigneeId}");
            }

            if (!assignee.IsAdmin && ((assignee.Department != null && assignee.DepartmentId != ticket.DepartmentId) ||
                                      assignee.Department == null))
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Исполнитель {assigneeId} не является администратором или из другого отдела",
                    assigneeId), ct);
                return Result<Guid>.Failure(
                    $"Исполнитель не является администратором: {assignee.Username} и его отдел: {assignee.Department?.Name}");
            }

            if (ticket.Status == TicketStatus.Rejected)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Тикет {id} уже отклонен",
                    assigneeId), ct);
                return Result<Guid>.Failure("Тикет уже отклонен");
            }

            ticket.Status = TicketStatus.Rejected;
            ticket.ClosedAt = DateTime.UtcNow;
            ticket.Feedback = feedback;
            ticket.AssigneeId ??= assigneeId;

            _database.TicketRepository.Update(ticket, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Тикет {id} успешно отклонен исполнителем {assigneeId}",
                    assigneeId), ct);
                return Result<Guid>.Success(ticket.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"Не было внесено изменений при отклонении тикета {id}",
                assigneeId), ct);
            return Result<Guid>.Failure("Не удалось обновить тикет");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при отклонении тикета {id}: {e.Message}. StackTrace: {e.StackTrace}",
                assigneeId), ct);
            return Result<Guid>.Failure($"Ошибка при обновлении тикета: {e.Message}");
        }
    }

    public async Task<Result<(int, int)>> GetTicketsCount(TicketType type, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Начало подсчета тикетов типа {type}",
                null), ct);

            var allTicketsCount = await _database.TicketRepository.CountAsync(
                ticket => ticket.Type == type &&
                          (ticket.Status == TicketStatus.Created || ticket.Status == TicketStatus.InWork),
                ct);

            var solvedTicketsCount = await _database.TicketRepository.CountAsync(
                ticket => ticket.Type == type &&
                          ticket.Status == TicketStatus.Completed,
                ct);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Подсчитано тикетов: {allTicketsCount} активных, {solvedTicketsCount} решенных",
                null), ct);

            return Result<(int, int)>.Success((allTicketsCount, solvedTicketsCount));
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при подсчете тикетов: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<(int, int)>.Failure($"Ошибка при подсчете тикетов: {e.Message}");
        }
    }
}