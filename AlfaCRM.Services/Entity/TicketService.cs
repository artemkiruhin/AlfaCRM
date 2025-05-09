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
                $"Starting ticket creation process. Request: {System.Text.Json.JsonSerializer.Serialize(request)}",
                request.CreatorId), ct);

            // Validate creator exists
            var creator = await _database.UserRepository.GetByIdAsync(request.CreatorId, ct);
            if (creator == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to create ticket: creator with ID {request.CreatorId} not found",
                    request.CreatorId), ct);
                return Result<Guid>.Failure("Creator not found");
            }

            // Validate department exists if specified
            
            var department = await _database.DepartmentRepository.GetByIdAsync(request.DepartmentId, ct);
            if (department == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to create ticket: department with ID {request.DepartmentId} not found",
                    request.CreatorId), ct);
                return Result<Guid>.Failure("Department not found");
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
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Ticket created successfully with ID: {ticket.Id}",
                    request.CreatorId), ct);
                return Result<Guid>.Success(ticket.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                "No changes were made when creating ticket",
                request.CreatorId), ct);
            return Result<Guid>.Failure("Failed to create ticket");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while creating ticket: {e.Message}. StackTrace: {e.StackTrace}",
                request?.CreatorId), ct);
            return Result<Guid>.Failure($"Error while creating ticket: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Update(TicketUpdateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting ticket update process for ID {request.Id}. Request: {System.Text.Json.JsonSerializer.Serialize(request)}",
                request.SenderId), ct);

            var ticket = await _database.TicketRepository.GetByIdAsync(request.Id, ct);
            if (ticket == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to update ticket: ticket with ID {request.Id} not found",
                    request.SenderId), ct);
                return Result<Guid>.Failure($"Ticket not found: {request.Id}");
            }

            var sender = await _database.UserRepository.GetByIdAsync(request.SenderId, ct);
            if (sender == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to update ticket: sender with ID {request.SenderId} not found",
                    request.SenderId), ct);
                return Result<Guid>.Failure($"Sender not found: {request.SenderId}");
            }
            
            if (sender is { IsAdmin: false, DepartmentId: null })
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to update ticket: sender {request.SenderId} does not have admin role",
                    request.SenderId), ct);
                return Result<Guid>.Failure("Sender does not have an admin role");
            }
            
            // if sender is creator
            if (ticket.CreatorId == request.SenderId && ticket.Status == TicketStatus.Created)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Updating ticket {request.Id} as creator",
                    request.SenderId), ct);

                if (!string.IsNullOrEmpty(request.Title)) ticket.Title = request.Title;
                if (!string.IsNullOrEmpty(request.Text)) ticket.Text = request.Text;
                if (request.DepartmentId.HasValue) ticket.DepartmentId = request.DepartmentId.Value;
            }

            // if sender is department expert
            if (sender.DepartmentId.HasValue && ticket.DepartmentId == sender.DepartmentId)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Updating ticket {request.Id} as department expert",
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
                    $"Updating ticket {request.Id} as admin",
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
                    $"Ticket {request.Id} updated successfully",
                    request.SenderId), ct);
                return Result<Guid>.Success(ticket.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"No changes were made when updating ticket {request.Id}",
                request.SenderId), ct);
            return Result<Guid>.Failure("Failed to update ticket");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while updating ticket {request?.Id}: {e.Message}. StackTrace: {e.StackTrace}",
                request?.SenderId), ct);
            return Result<Guid>.Failure($"Error while updating ticket: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id, Guid userId, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting ticket deletion process for ID {id} by user {userId}",
                userId), ct);

            var ticket = await _database.TicketRepository.GetByIdAsync(id, ct);
            if (ticket == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to delete ticket: ticket with ID {id} not found",
                    userId), ct);
                return Result<Guid>.Failure($"Ticket not found: {id}");
            }
        
            var user = await _database.UserRepository.GetByIdAsync(userId, ct);
            if (user == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to delete ticket: user with ID {userId} not found",
                    userId), ct);
                return Result<Guid>.Failure($"User not found: {userId}");
            }
        
            if (!user.IsAdmin)
            {
                if (ticket.CreatorId != userId)
                {
                    await _database.LogRepository.CreateAsync(LogEntity.Create(
                        LogType.Warning,
                        $"User {userId} is not admin or ticket creator",
                        userId), ct);
                    return Result<Guid>.Failure("Only admin or ticket creator can delete ticket");
                }
            
                if (ticket.Status != TicketStatus.Created)
                {
                    await _database.LogRepository.CreateAsync(LogEntity.Create(
                        LogType.Warning,
                        $"Ticket {id} is not in 'Created' status (current status: {ticket.Status})",
                        userId), ct);
                    return Result<Guid>.Failure("Creator can only delete tickets in 'Created' status");
                }
            }
        
            _database.TicketRepository.Delete(ticket, ct);
            var result = await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);

            if (result > 0)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Info,
                    $"Ticket {id} deleted successfully by user {userId}",
                    userId), ct);
                return Result<Guid>.Success(ticket.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"No changes were made when deleting ticket {id}",
                userId), ct);
            return Result<Guid>.Failure("Failed to delete ticket");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while deleting ticket {id}: {e.Message}. StackTrace: {e.StackTrace}",
                userId), ct);
            return Result<Guid>.Failure($"Error while deleting ticket: {e.Message}");
        }
    }

    public async Task<Result<List<TicketShortDTO>>> GetAllShort(Guid? departmentId, Guid? senderId, TicketType? type, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to retrieve all short tickets (Department: {departmentId}, Sender: {senderId}, Type: {type})",
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
                DepartmentName: ticket.Department?.Name ?? "No department",
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
                $"Retrieved {dtos.Count} short tickets",
                null), ct);
            
            return Result<List<TicketShortDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while retrieving short tickets: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<List<TicketShortDTO>>.Failure($"Error while retrieving tickets: {e.Message}");
        }
    }

    public async Task<Result<List<TicketDetailedDTO>>> GetAll(Guid? departmentId, Guid? senderId, TicketType? type, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to retrieve all detailed tickets (Department: {departmentId}, Sender: {senderId}, Type: {type})",
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
                Department: ticket.DepartmentId != null ? new DepartmentShortDTO(
                    Id: ticket.DepartmentId,
                    Name: ticket.Department?.Name ?? string.Empty,
                    MembersCount: ticket.Department?.Users.Count ?? 0,
                    IsSpecific: ticket.Department?.IsSpecific ?? false
                ) : null,
                CreatedAt: ticket.CreatedAt,
                Status: ticket.Status switch
                {
                    TicketStatus.Created => "Создано",
                    TicketStatus.InWork => "В работе",
                    TicketStatus.Completed => "Выполнено",
                    TicketStatus.Rejected => "Отменено",
                    _ => throw new ArgumentOutOfRangeException(nameof(ticket.Status))
                },
                Assignee: ticket.Assignee == null ? null : new UserShortDTO(
                    Id: ticket.Assignee.Id,
                    FullName: ticket.Assignee.FullName,
                    Username: ticket.Assignee.Username,
                    Email: ticket.Assignee.Email,
                    DepartmentName: ticket.Assignee?.Department?.Name ?? "Нет отдела",
                    IsAdmin: ticket.Assignee?.IsAdmin ?? false,
                    IsBlocked: ticket.Assignee?.IsBlocked ?? false
                ),
                Creator: new UserShortDTO(
                    Id: ticket.Creator.Id,
                    FullName: ticket.Creator.FullName,
                    Username: ticket.Creator.Username,
                    Email: ticket.Creator.Email,
                    DepartmentName: ticket.Creator?.Department?.Name ?? "Нет отдела",
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
                $"Retrieved {dtos.Count} detailed tickets",
                null), ct);
            
            return Result<List<TicketDetailedDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while retrieving detailed tickets: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<List<TicketDetailedDTO>>.Failure($"Error while retrieving tickets: {e.Message}");
        }
    }

    public async Task<Result<TicketDetailedDTO>> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to retrieve ticket with ID {id}",
                null), ct);

            var ticket = await _database.TicketRepository.GetByIdAsync(id, ct);
            if (ticket == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Ticket with ID {id} not found",
                    null), ct);
                return Result<TicketDetailedDTO>.Failure($"Ticket not found: {id}");
            }

            var dto = new TicketDetailedDTO(
                Id: ticket.Id,
                Title: ticket.Title,
                Text: ticket.Text,
                Feedback: ticket.Feedback,
                Department: ticket.DepartmentId != null ? new DepartmentShortDTO(
                    Id: ticket.DepartmentId,
                    Name: ticket.Department?.Name ?? string.Empty,
                    MembersCount: ticket.Department?.Users.Count ?? 0,
                    IsSpecific: ticket.Department?.IsSpecific ?? false
                ) : null,
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
                        DepartmentName: ticket.Assignee?.Department?.Name ?? "Нет отдела",
                        IsAdmin: ticket.Assignee?.IsAdmin ?? false,
                        IsBlocked: ticket.Assignee?.IsBlocked ?? false
                    ),
                Creator: new UserShortDTO(
                    Id: ticket.Creator.Id,
                    FullName: ticket.Creator.FullName,
                    Username: ticket.Creator.Username,
                    Email: ticket.Creator.Email,
                    DepartmentName: ticket.Creator?.Department?.Name ?? "Нет отдела",
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
                $"Successfully retrieved ticket with ID {id}",
                null), ct);

            return Result<TicketDetailedDTO>.Success(dto);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while retrieving ticket {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<TicketDetailedDTO>.Failure($"Error while retrieving ticket: {e.Message}");
        }
    }

    public async Task<Result<TicketShortDTO>> GetByIdShort(Guid id, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to retrieve short ticket with ID {id}",
                null), ct);

            var ticket = await _database.TicketRepository.GetByIdAsync(id, ct);
            if (ticket == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Ticket with ID {id} not found",
                    null), ct);
                return Result<TicketShortDTO>.Failure($"Ticket not found: {id}");
            }

            var dto = new TicketShortDTO(
                Id: ticket.Id,
                Title: ticket.Title,
                Feedback: ticket.Feedback,
                DepartmentId: ticket.DepartmentId,
                DepartmentName: ticket.Department?.Name ?? "No department",
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
                $"Successfully retrieved short ticket with ID {id}",
                null), ct);

            return Result<TicketShortDTO>.Success(dto);
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while retrieving short ticket {id}: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<TicketShortDTO>.Failure($"Error while retrieving ticket: {e.Message}");
        }
    }

    public async Task<Result<Guid>> TakeToWork(Guid id, Guid assigneeId, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to take ticket {id} to work by assignee {assigneeId}",
                assigneeId), ct);

            var ticket = await _database.TicketRepository.GetByIdAsync(id, ct);
            if (ticket == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to take ticket to work: ticket with ID {id} not found",
                    assigneeId), ct);
                return Result<Guid>.Failure($"Ticket not found: {id}");
            }
            
            var assignee = await _database.UserRepository.GetByIdAsync(assigneeId, ct);
            if (assignee == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to take ticket to work: assignee with ID {assigneeId} not found",
                    assigneeId), ct);
                return Result<Guid>.Failure($"Assignee not found: {assigneeId}");
            }
            
            if (!assignee.IsAdmin && ((assignee.Department != null && assignee.DepartmentId != ticket.DepartmentId) || assignee.Department == null))
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Assignee {assigneeId} is not admin or from different department",
                    assigneeId), ct);
                return Result<Guid>.Failure($"Assignee is not an admin: {assignee.Username} and its department: {assignee.Department?.Name}");
            }
            
            if (ticket.Status == TicketStatus.InWork)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Ticket {id} is already in work",
                    assigneeId), ct);
                return Result<Guid>.Failure("Ticket is already in work");
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
                    $"Ticket {id} taken to work successfully by assignee {assigneeId}",
                    assigneeId), ct);
                return Result<Guid>.Success(ticket.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"No changes were made when taking ticket {id} to work",
                assigneeId), ct);
            return Result<Guid>.Failure("Failed to update ticket");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while taking ticket {id} to work: {e.Message}. StackTrace: {e.StackTrace}",
                assigneeId), ct);
            return Result<Guid>.Failure($"Error while updating ticket: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Complete(Guid id, Guid assigneeId, string feedback, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to complete ticket {id} by assignee {assigneeId}",
                assigneeId), ct);

            var ticket = await _database.TicketRepository.GetByIdAsync(id, ct);
            if (ticket == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to complete ticket: ticket with ID {id} not found",
                    assigneeId), ct);
                return Result<Guid>.Failure($"Ticket not found: {id}");
            }
            
            var assignee = await _database.UserRepository.GetByIdAsync(assigneeId, ct);
            if (assignee == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to complete ticket: assignee with ID {assigneeId} not found",
                    assigneeId), ct);
                return Result<Guid>.Failure($"Assignee not found: {assigneeId}");
            }
            
            if (!assignee.IsAdmin && ((assignee.Department != null && assignee.DepartmentId != ticket.DepartmentId) || assignee.Department == null))
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Assignee {assigneeId} is not admin or from different department",
                    assigneeId), ct);
                return Result<Guid>.Failure($"Assignee is not an admin: {assignee.Username} and its department: {assignee.Department?.Name}");
            }
            
            if (ticket.Status == TicketStatus.Completed)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Ticket {id} is already completed",
                    assigneeId), ct);
                return Result<Guid>.Failure("Ticket is already completed");
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
                    $"Ticket {id} completed successfully by assignee {assigneeId}",
                    assigneeId), ct);
                return Result<Guid>.Success(ticket.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"No changes were made when completing ticket {id}",
                assigneeId), ct);
            return Result<Guid>.Failure("Failed to update ticket");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while completing ticket {id}: {e.Message}. StackTrace: {e.StackTrace}",
                assigneeId), ct);
            return Result<Guid>.Failure($"Error while updating ticket: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Reject(Guid id, Guid assigneeId, string feedback, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to reject ticket {id} by assignee {assigneeId}",
                assigneeId), ct);

            var ticket = await _database.TicketRepository.GetByIdAsync(id, ct);
            if (ticket == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to reject ticket: ticket with ID {id} not found",
                    assigneeId), ct);
                return Result<Guid>.Failure($"Ticket not found: {id}");
            }
            
            var assignee = await _database.UserRepository.GetByIdAsync(assigneeId, ct);
            if (assignee == null)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Failed to reject ticket: assignee with ID {assigneeId} not found",
                    assigneeId), ct);
                return Result<Guid>.Failure($"Assignee not found: {assigneeId}");
            }
            
            if (!assignee.IsAdmin && ((assignee.Department != null && assignee.DepartmentId != ticket.DepartmentId) || assignee.Department == null))
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Assignee {assigneeId} is not admin or from different department",
                    assigneeId), ct);
                return Result<Guid>.Failure($"Assignee is not an admin: {assignee.Username} and its department: {assignee.Department?.Name}");
            }
            
            if (ticket.Status == TicketStatus.Rejected)
            {
                await _database.LogRepository.CreateAsync(LogEntity.Create(
                    LogType.Warning,
                    $"Ticket {id} is already rejected",
                    assigneeId), ct);
                return Result<Guid>.Failure("Ticket is already rejected");
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
                    $"Ticket {id} rejected successfully by assignee {assigneeId}",
                    assigneeId), ct);
                return Result<Guid>.Success(ticket.Id);
            }

            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Warning,
                $"No changes were made when rejecting ticket {id}",
                assigneeId), ct);
            return Result<Guid>.Failure("Failed to update ticket");
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while rejecting ticket {id}: {e.Message}. StackTrace: {e.StackTrace}",
                assigneeId), ct);
            return Result<Guid>.Failure($"Error while updating ticket: {e.Message}");
        }
    }

    public async Task<Result<(int, int)>> GetTicketsCount(TicketType type, CancellationToken ct)
    {
        try
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Info,
                $"Starting to count tickets of type {type}",
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
                $"Counted tickets: {allTicketsCount} active, {solvedTicketsCount} solved",
                null), ct);

            return Result<(int, int)>.Success((allTicketsCount, solvedTicketsCount));
        }
        catch (Exception e)
        {
            await _database.LogRepository.CreateAsync(LogEntity.Create(
                LogType.Error,
                $"Error while counting tickets: {e.Message}. StackTrace: {e.StackTrace}",
                null), ct);
            return Result<(int, int)>.Failure($"Error while counting tickets: {e.Message}");
        }
    }
}