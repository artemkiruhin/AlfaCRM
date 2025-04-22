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
            var ticket = TicketEntity.Create(request.Title, request.Text, request.DepartmentId, TicketStatus.Created, null, null, request.CreatorId, request.Type);
            await _database.TicketRepository.CreateAsync(ticket, ct);
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);
            return Result<Guid>.Success(ticket.Id);
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<Guid>.Failure($"Error while creating ticket: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Update(TicketUpdateRequest request, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            var ticket = await _database.TicketRepository.GetByIdAsync(request.Id, ct);
            if (ticket == null) return Result<Guid>.Failure($"Ticket not found: {request.Id}");

            var sender = await _database.UserRepository.GetByIdAsync(request.SenderId, ct);
            if (sender == null) return Result<Guid>.Failure($"Sender not found: {request.SenderId}");
            
            if (sender is { IsAdmin: false, DepartmentId: null })
                return Result<Guid>.Failure($"Sender does not have an admin role");
            
            // if sender is creator
            if (ticket.CreatorId == request.SenderId && ticket.Status == TicketStatus.Created)
            {
                if (!string.IsNullOrEmpty(request.Title)) ticket.Title = request.Title;
                if (!string.IsNullOrEmpty(request.Text)) ticket.Text = request.Text;
                if (request.DepartmentId.HasValue) ticket.DepartmentId = request.DepartmentId.Value;
            }

            // if sender is department expert
            if (sender.DepartmentId.HasValue && ticket.DepartmentId == sender.DepartmentId)
            {
                if (ticket.Status is TicketStatus.Completed or TicketStatus.Rejected)
                {
                    if (!string.IsNullOrEmpty(request.Feedback)) ticket.Feedback = request.Feedback;
                }
            }

            if (sender.IsAdmin)
            {
                if (!string.IsNullOrEmpty(request.Title)) ticket.Title = request.Title;
                if (!string.IsNullOrEmpty(request.Text)) ticket.Text = request.Text;
                if (request.DepartmentId.HasValue) ticket.DepartmentId = request.DepartmentId.Value;
                if (!string.IsNullOrEmpty(request.Feedback)) ticket.Feedback = request.Feedback;
                if (request.Type.HasValue) ticket.Type = request.Type.Value;
            }

            _database.TicketRepository.Update(ticket, ct);
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);
            return Result<Guid>.Success(ticket.Id);
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<Guid>.Failure($"Error while updating ticket: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Delete(Guid id, Guid userId, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            var ticket = await _database.TicketRepository.GetByIdAsync(id, ct);
            if (ticket == null) 
                return Result<Guid>.Failure($"Ticket not found: {id}");
        
            var user = await _database.UserRepository.GetByIdAsync(userId, ct);
            if (user == null) 
                return Result<Guid>.Failure($"User not found: {userId}");
        
            if (!user.IsAdmin)
            {
                if (ticket.CreatorId != userId)
                    return Result<Guid>.Failure("Only admin or ticket creator can delete ticket");
            
                if (ticket.Status != TicketStatus.Created)
                    return Result<Guid>.Failure("Creator can only delete tickets in 'Created' status");
            }
        
            _database.TicketRepository.Delete(ticket, ct);
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);
        
            return Result<Guid>.Success(ticket.Id);
        }
        catch (Exception e)
        {
            await _database.RollbackTransactionAsync(ct);
            return Result<Guid>.Failure($"Error while deleting ticket: {e.Message}");
        }
    }

    public async Task<Result<List<TicketShortDTO>>> GetAllShort(Guid? departmentId, Guid? senderId, TicketType? type, CancellationToken ct)
    {
        try
        {
            var tickets = await _database.TicketRepository.GetAllAsync(ct);
            
            if (departmentId.HasValue) tickets = tickets.Where(ticket => ticket.DepartmentId == departmentId);
            if (senderId.HasValue) tickets = tickets.Where(ticket => ticket.CreatorId == senderId);
            if (type.HasValue) tickets = tickets.Where(ticket => ticket.Type == type);

            var dtos = tickets.Select(ticket => new TicketShortDTO(
                Id: ticket.Id,
                Title: ticket.Title,
                Feedback: ticket.Feedback,
                DepartmentId: ticket.DepartmentId,
                DepartmentName: ticket.Department.Name,
                CreatedAt: ticket.CreatedAt,
                Status: ticket.Status switch
                {
                    TicketStatus.Created => "Создано",
                    TicketStatus.InWork => "В работе",
                    TicketStatus.Completed => "Выполнено",
                    TicketStatus.Rejected => "Отменено",
                    _ => throw new ArgumentOutOfRangeException(nameof(ticket.Status))
                },
                AssigneeId: ticket.AssigneeId.Value,
                AssigneeUsername: ticket.Assignee.Username,
                ClosedAt: ticket.ClosedAt,
                CreatorId: ticket.CreatorId,
                CreatorUsername: ticket.Creator.Username,
                Type: ticket.Type switch
                {
                    TicketType.ProblemCase => "Заявка",
                    TicketType.Suggestion => "Предложение",
                    _ => throw new ArgumentOutOfRangeException(nameof(ticket.Type))
                }
            )).ToList();

            return Result<List<TicketShortDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            return Result<List<TicketShortDTO>>.Failure($"Error while retrieving tickets: {e.Message}");
        }
    }

    public async Task<Result<List<TicketDetailedDTO>>> GetAll(Guid? departmentId, Guid? senderId, TicketType? type, CancellationToken ct)
    {
        try
        {
            var tickets = await _database.TicketRepository.GetAllAsync(ct);
            
            if (departmentId.HasValue) tickets = tickets.Where(ticket => ticket.DepartmentId == departmentId);
            if (senderId.HasValue) tickets = tickets.Where(ticket => ticket.CreatorId == senderId);
            if (type.HasValue) tickets = tickets.Where(ticket => ticket.Type == type);
            
            var dtos = tickets.Select(ticket => new TicketDetailedDTO(
                Id: ticket.Id,
                Title: ticket.Title,
                Text: ticket.Text,
                Feedback: ticket.Feedback,
                Department: new DepartmentShortDTO(
                    Id: ticket.DepartmentId,
                    Name: ticket.Department.Name,
                    MembersCount: ticket.Department.Users.Count,
                    IsSpecific: ticket.Department.IsSpecific
                    ),
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

            return Result<List<TicketDetailedDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            return Result<List<TicketDetailedDTO>>.Failure($"Error while retrieving tickets: {e.Message}");
        }
    }

    public async Task<Result<TicketDetailedDTO>> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var ticket = await _database.TicketRepository.GetByIdAsync(id, ct);
            if (ticket == null) return Result<TicketDetailedDTO>.Failure($"Ticket not found: {id}");

            var dto = new TicketDetailedDTO(
                Id: ticket.Id,
                Title: ticket.Title,
                Text: ticket.Text,
                Feedback: ticket.Feedback,
                Department: new DepartmentShortDTO(
                    Id: ticket.DepartmentId,
                    Name: ticket.Department.Name,
                    MembersCount: ticket.Department.Users.Count,
                    IsSpecific: ticket.Department.IsSpecific
                ),
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

            return Result<TicketDetailedDTO>.Success(dto);
        }
        catch (Exception e)
        {
            return Result<TicketDetailedDTO>.Failure($"Error while retrieving tickets: {e.Message}");
        }
    }

    public async Task<Result<TicketShortDTO>> GetByIdShort(Guid id, CancellationToken ct)
    {
        try
        {
            var ticket = await _database.TicketRepository.GetByIdAsync(id, ct);
            if (ticket == null) return Result<TicketShortDTO>.Failure($"Ticket not found: {id}");

            var dto = new TicketShortDTO(
                Id: ticket.Id,
                Title: ticket.Title,
                Feedback: ticket.Feedback,
                DepartmentId: ticket.DepartmentId,
                DepartmentName: ticket.Department.Name,
                CreatedAt: ticket.CreatedAt,
                Status: ticket.Status switch
                {
                    TicketStatus.Created => "Создано",
                    TicketStatus.InWork => "В работе",
                    TicketStatus.Completed => "Выполнено",
                    TicketStatus.Rejected => "Отменено",
                    _ => throw new ArgumentOutOfRangeException(nameof(ticket.Status))
                },
                AssigneeId: ticket.AssigneeId.Value,
                AssigneeUsername: ticket.Assignee.Username,
                ClosedAt: ticket.ClosedAt,
                CreatorId: ticket.CreatorId,
                CreatorUsername: ticket.Creator.Username,
                Type: ticket.Type switch
                {
                    TicketType.ProblemCase => "Заявка",
                    TicketType.Suggestion => "Предложение",
                    _ => throw new ArgumentOutOfRangeException(nameof(ticket.Type))
                }
            );
            return Result<TicketShortDTO>.Success(dto);
        }
        catch (Exception e)
        {
            return Result<TicketShortDTO>.Failure($"Error while retrieving ticket: {e.Message}");
        }
    }

    public async Task<Result<Guid>> TakeToWork(Guid id, Guid assigneeId, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            var ticket = await _database.TicketRepository.GetByIdAsync(id, ct);
            if (ticket == null) return Result<Guid>.Failure($"Ticket not found: {id}");
            
            var assignee = await _database.UserRepository.GetByIdAsync(assigneeId, ct);
            if (assignee == null) return Result<Guid>.Failure($"Assignee not found: {id}");
            
            if (!assignee.IsAdmin && ((assignee.Department != null && assignee.DepartmentId != ticket.DepartmentId) || assignee.Department == null))
                return Result<Guid>.Failure($"Assignee is not an admin: {assignee.Username} and its department: {assignee.Department?.Name}");
            
            if (ticket.Status == TicketStatus.InWork) return Result<Guid>.Failure("Ticket is already in work");
            ticket.Status = TicketStatus.InWork;
            ticket.AssigneeId = assigneeId;
            
            _database.TicketRepository.Update(ticket, ct);
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);
            return Result<Guid>.Success(ticket.Id);
        }
        catch (Exception e)
        {
            return Result<Guid>.Failure($"Error while updating ticket: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Complete(Guid id, Guid assigneeId, string feedback, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            var ticket = await _database.TicketRepository.GetByIdAsync(id, ct);
            if (ticket == null) return Result<Guid>.Failure($"Ticket not found: {id}");
            
            var assignee = await _database.UserRepository.GetByIdAsync(assigneeId, ct);
            if (assignee == null) return Result<Guid>.Failure($"Assignee not found: {id}");
            
            if (!assignee.IsAdmin && ((assignee.Department != null && assignee.DepartmentId != ticket.DepartmentId) || assignee.Department == null))
                return Result<Guid>.Failure($"Assignee is not an admin: {assignee.Username} and its department: {assignee.Department?.Name}");
            
            if (ticket.Status == TicketStatus.Completed) return Result<Guid>.Failure("Ticket is already in work");
            ticket.Status = TicketStatus.Completed;
            ticket.ClosedAt = DateTime.UtcNow;
            ticket.Feedback = feedback;
            ticket.AssigneeId ??= assigneeId;
            
            _database.TicketRepository.Update(ticket, ct);
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);
            return Result<Guid>.Success(ticket.Id);
        }
        catch (Exception e)
        {
            return Result<Guid>.Failure($"Error while updating ticket: {e.Message}");
        }
    }

    public async Task<Result<Guid>> Reject(Guid id, Guid assigneeId, string feedback, CancellationToken ct)
    {
        await _database.BeginTransactionAsync(ct);
        try
        {
            var ticket = await _database.TicketRepository.GetByIdAsync(id, ct);
            if (ticket == null) return Result<Guid>.Failure($"Ticket not found: {id}");
            
            var assignee = await _database.UserRepository.GetByIdAsync(assigneeId, ct);
            if (assignee == null) return Result<Guid>.Failure($"Assignee not found: {id}");
            
            if (!assignee.IsAdmin && ((assignee.Department != null && assignee.DepartmentId != ticket.DepartmentId) || assignee.Department == null))
                return Result<Guid>.Failure($"Assignee is not an admin: {assignee.Username} and its department: {assignee.Department?.Name}");
            
            if (ticket.Status == TicketStatus.Rejected) return Result<Guid>.Failure("Ticket is already in work");
            ticket.Status = TicketStatus.Rejected;
            ticket.ClosedAt = DateTime.UtcNow;
            ticket.Feedback = feedback;
            ticket.AssigneeId ??= assigneeId;
            
            _database.TicketRepository.Update(ticket, ct);
            await _database.SaveChangesAsync(ct);
            await _database.CommitTransactionAsync(ct);
            return Result<Guid>.Success(ticket.Id);
        }
        catch (Exception e)
        {
            return Result<Guid>.Failure($"Error while updating ticket: {e.Message}");
        }
    }
}