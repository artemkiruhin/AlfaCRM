using AlfaCRM.Domain.Interfaces.Database;
using AlfaCRM.Domain.Interfaces.Services.Extensions;
using AlfaCRM.Domain.Models.DTOs;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Services.Extensions;

public class StatisticsService : IStatisticsService
{
    private readonly IUnitOfWork _database;

    public StatisticsService(IUnitOfWork database)
    {
        _database = database;
    }

    public async Task<Result<bool>> DistributeTicketsByUsers(CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                "Начало автоматического распределения заявок",
                null), ct);

            await _database.BeginTransactionAsync(ct);
            
            try
            {
                var departments = await _database.DepartmentRepository.GetDepartmentsBySpecific(true, ct);
                
                int totalDistributed = 0;
                
                foreach (var department in departments)
                {
                    var problemCases = await _database.TicketRepository.FindRangeAsync(ticket =>
                            ticket.DepartmentId == department.Id
                            && ticket.Type == TicketType.ProblemCase
                            && ticket.Status == TicketStatus.Created
                            && ticket.AssigneeId == null,
                        ct);

                    var suggestions = await _database.TicketRepository.FindRangeAsync(ticket =>
                            ticket.DepartmentId == department.Id
                            && ticket.Type == TicketType.Suggestion
                            && ticket.Status == TicketStatus.Created
                            && ticket.AssigneeId == null,
                        ct);

                    var allTickets = problemCases.Concat(suggestions).ToList();
                    if (!allTickets.Any()) continue;
                    
                    await _database.LogRepository.CreateAsync(LogEntity.Create(
                        LogType.Info,
                        $"Найдено {allTickets.Count} нераспределенных заявок в отделе {department.Name}",
                        null), ct);

                    var shuffledTickets = ShuffleTickets(allTickets).ToList();
                    
                    var activeUsers = department.Users
                        .Where(u => u.IsActive) 
                        .Select(u => new {
                            User = u,
                            TicketCount = u.AssignedTickets
                                .Count(ticket => ticket.Status == TicketStatus.Created)
                        })
                        .OrderBy(x => x.TicketCount)
                        .ToList();

                    if (!activeUsers.Any())
                    {
                        await _database.LogRepository.CreateAsync(LogEntity.Create(
                            LogType.Warning,
                            $"В отделе {department.Name} нет доступных пользователей для распределения заявок",
                            null), ct);
                        continue;
                    }
                    
                    await _database.LogRepository.CreateAsync(LogEntity.Create(
                        LogType.Info,
                        $"Найдено {activeUsers.Count} активных пользователей в отделе {department.Name}",
                        null), ct);

                    int distributedInDepartment = 0;
                    for (int i = 0; i < shuffledTickets.Count; i++)
                    {
                        var userIndex = i % activeUsers.Count;
                        var user = activeUsers[userIndex].User;
                        var ticket = shuffledTickets[i];

                        ticket.AssigneeId = user.Id;
                        ticket.Status = TicketStatus.InWork;
                        
                        _database.TicketRepository.Update(ticket, ct);
                        
                        activeUsers[userIndex] = new {
                            User = activeUsers[userIndex].User,
                            TicketCount = activeUsers[userIndex].TicketCount + 1
                        };
                        
                        activeUsers = activeUsers.OrderBy(x => x.TicketCount).ToList();
                        
                        distributedInDepartment++;
                    }
                    
                    totalDistributed += distributedInDepartment;
                    
                    await _database.LogRepository.CreateAsync(LogEntity.Create(
                        LogType.Info,
                        $"В отделе {department.Name} распределено {distributedInDepartment} заявок",
                        null), ct);
                }
                
                await _database.SaveChangesAsync(ct);
                
                
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Автоматическое распределение заявок завершено. Всего распределено: {totalDistributed}",
                    null), ct);

                if (totalDistributed == 0)
                {
                    return Result<bool>.Success(true);
                }

                return Result<bool>.Success(true);
            }
            catch (Exception innerEx)
            {
                await _database.RollbackTransactionAsync(ct);
                throw new Exception($"Ошибка при распределении заявок: {innerEx.Message}", innerEx);
            }
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при распределении заявок: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<bool>.Failure($"Ошибка при распределении заявок: {e.Message}");
        }
    }
    
    public async Task<Result<bool>> DistributeTicketToUser(Guid userId, Guid ticketId, CancellationToken ct)
    {
        try
        {
            var ticket = await _database.TicketRepository.GetByIdAsync(ticketId, ct);
            if (ticket == null)
                return Result<bool>.Failure("Заявка не найдена");

            var user = await _database.UserRepository.GetByIdAsync(userId, ct);
            if (user == null)
                return Result<bool>.Failure("Пользователь не найден");
                
            if (!user.IsActive)
                return Result<bool>.Failure("Пользователь неактивен и не может получать заявки");

            if (ticket.AssigneeId != null && ticket.Status != TicketStatus.Created)
                return Result<bool>.Failure("Заявка уже назначена другому пользователю или находится в работе");

            ticket.AssigneeId = userId;
            ticket.Status = TicketStatus.InWork;
            
            _database.TicketRepository.Update(ticket, ct);
            await _database.SaveChangesAsync(ct);

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Заявка {ticketId} назначена пользователю {userId}",
                userId), ct);

            return Result<bool>.Success(true);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка назначения заявки {ticketId} пользователю {userId}: {e.Message}",
                null), ct);
            return Result<bool>.Failure($"Ошибка назначения заявки: {e.Message}");
        }
    }
    
    public async Task<Result<IEnumerable<UsersPerDepartmentByTicketsDTO>>> GetUsersByTicketBusiness(CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                "Получение пользователей по загруженности заявками",
                null), ct);
                
            var departments = await _database.DepartmentRepository.GetDepartmentsBySpecific(true, ct);

            var dtos = departments.Select(department =>
            {
                var users = department.Users
                    .Where(user => user.IsActive) 
                    .Select(user => new UsersByTicketBusinessDTO
                    {
                        UserId = user.Id,
                        Fullname = user.FullName,
                        SuggestionsCount = user.AssignedTickets.Count(ticket => 
                            ticket.Type == TicketType.Suggestion && 
                            ticket.Status == TicketStatus.Created),
                        TicketsCount = user.AssignedTickets.Count(ticket => 
                            ticket.Type == TicketType.ProblemCase && 
                            ticket.Status == TicketStatus.Created), 
                        Username = user.Username
                    })
                    .OrderByDescending(user =>
                        user.SuggestionsCount + user.TicketsCount
                    )
                    .ToList();

                return new UsersPerDepartmentByTicketsDTO
                {
                    DepartmentId = department.Id,
                    DepartmentName = department.Name,
                    Users = users
                };
            }).ToList();

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                "Успешно получены пользователи по загруженности заявками",
                null), ct);

            return Result<IEnumerable<UsersPerDepartmentByTicketsDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Ошибка при получении пользователей по загруженности: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<IEnumerable<UsersPerDepartmentByTicketsDTO>>.Failure($"Ошибка при получении информации: {e.Message}");
        }
    }
    
    private IEnumerable<T> ShuffleTickets<T>(IEnumerable<T> source)
    {
        var items = source.ToList();
        var random = new Random();
        
        for (int i = items.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            T temp = items[i];
            items[i] = items[j];
            items[j] = temp;
        }
        
        return items;
    }
}