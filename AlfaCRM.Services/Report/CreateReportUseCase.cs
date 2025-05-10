using AlfaCRM.Domain.Interfaces.Database;
using AlfaCRM.Domain.Models.DTOs;
using AlfaCRM.Domain.Models.DTOs.Report;
using AlfaCRM.Domain.Models.Entities;
using AlfaCRM.Domain.Models.Settings;

namespace AlfaCRM.Services.Report;

public class CreateReportUseCase
{
    private readonly ExcelReportService _excelReportService;
    private readonly IUnitOfWork _database;

    public CreateReportUseCase(ExcelReportService excelReportService, IUnitOfWork database)
    {
        _excelReportService = excelReportService;
        _database = database;
    }

    public async Task<Result<byte[]>> Exec(ReportTableNumber table, string? description, CancellationToken ct)
    {
        try
        {
            return table switch
            {
                ReportTableNumber.Users => await CreateUsersReport(description, ct),
                ReportTableNumber.Departments => await CreateDepartmentsReport(description, ct),
                ReportTableNumber.Posts => await CreatePostsReport(description, ct),
                ReportTableNumber.Tickets => await CreateTicketsReport(description, ct),
                ReportTableNumber.Suggestions => await CreateTicketsByTypeReport(TicketType.Suggestion, description, ct),
                ReportTableNumber.ProblemCases => await CreateTicketsByTypeReport(TicketType.ProblemCase, description, ct),
                ReportTableNumber.Logs => await CreateLogsReport(description, ct),
                _ => throw new ApplicationException($"Unknown table type: {table}")
            };
        }
        catch (Exception e)
        {
            return Result<byte[]>.Failure($"Failed to create {table} report. Error: {e.Message}");
        }
    }

    private async Task<Result<byte[]>> CreateLogsReport(string? description, CancellationToken ct)
    {
        try
        {
            var logs = await _database.LogRepository.GetAllAsync(ct);
            var dtos = logs.Select(entity => new LogReportDTO(
                Id: entity.Id,
                Message: entity.Message,
                Type: entity.Type.ToString(),
                UserIdString: entity.UserId?.ToString() ?? "",
                Username: entity.User?.Username ?? "System",
                CreatedAt: entity.CreatedAt
            )).OrderByDescending(x => x.CreatedAt).ToList();
        
            var fileContent = _excelReportService.GenerateLogsReport(dtos, description);
            return Result<byte[]>.Success(fileContent);
        }
        catch (Exception e)
        {
            return Result<byte[]>.Failure($"Failed to generate logs report. Error: {e.Message}");
        }
    }
    

    private async Task<Result<byte[]>> CreateDepartmentsReport(string? description, CancellationToken ct)
    {
        try
        {
            var departments = await _database.DepartmentRepository.GetAllAsync(ct);
            var dtos = departments.Select(entity => new DepartmentReportDTO(
                Id: entity.Id,
                Name: entity.Name,
                IsSpecific: entity.IsSpecific,
                MembersCount: entity.Users.Count
            )).ToList();
            
            var fileContent = _excelReportService.GenerateDepartmentsReport(dtos, description);
            return Result<byte[]>.Success(fileContent);
        }
        catch (Exception e)
        {
            return Result<byte[]>.Failure($"Failed to generate departments report. Error: {e.Message}");
        }
    }
    
    private async Task<Result<byte[]>> CreatePostsReport(string? description, CancellationToken ct)
    {
        try
        {
            var posts = await _database.PostRepository.GetAllAsync(ct);
            var dtos = posts.Select(entity => new PostReportDTO(
                Id: entity.Id,
                Title: entity.Title,
                Content: entity.Content,
                Subtitle: entity.Subtitle ?? "",
                CreatedAt: entity.CreatedAt,
                ModifiedAt: entity.ModifiedAt,
                IsImportant: entity.IsImportant,
                IsActual: entity.IsActual,
                PublisherUsername: entity.Publisher.Username,
                PublisherFullName: entity.Publisher.FullName,
                DepartmentGuid: entity.DepartmentId?.ToString() ?? "",
                DepartmentName: entity.Department?.Name ?? "",
                LikesCount: entity.Reactions.Count(r => r.Type == ReactionType.Like),
                DislikesCount: entity.Reactions.Count(r => r.Type == ReactionType.Dislike),
                CommentsCount: entity.Comments.Count
            )).ToList();
            
            var fileContent = _excelReportService.GeneratePostsReport(dtos, description);
            return Result<byte[]>.Success(fileContent);
        }
        catch (Exception e)
        {
            return Result<byte[]>.Failure($"Failed to generate posts report. Error: {e.Message}");
        }
    }
    
    private async Task<Result<byte[]>> CreateTicketsReport(string? description, CancellationToken ct)
    {
        try
        {
            var tickets = await _database.TicketRepository.GetAllAsync(ct);
            var dtos = tickets.Select(entity => new TicketReportDTO(
                Id: entity.Id,
                Title: entity.Title,
                Text: entity.Text,
                Feedback: entity.Feedback ?? "",
                DepartmentGuid: entity.DepartmentId.ToString(),
                DepartmentName: entity.Department.Name,
                CreatedAt: entity.CreatedAt,
                Status: GetTicketStatus(entity.Status),
                AssigneeUsername: entity.Assignee?.Username ?? "",
                AssigneeFullName: entity.Assignee?.FullName ?? "",
                CreatorUsername: entity.Creator.Username,
                CreatorFullName: entity.Creator.FullName,
                ClosedAt: entity.ClosedAt,
                Type: GetTicketType(entity.Type)
            )).ToList();
            
            var fileContent = _excelReportService.GenerateTicketsReport(dtos, description);
            return Result<byte[]>.Success(fileContent);
        }
        catch (Exception e)
        {
            return Result<byte[]>.Failure($"Failed to generate tickets report. Error: {e.Message}");
        }
    }
    
    private async Task<Result<byte[]>> CreateTicketsByTypeReport(TicketType type, string? description, CancellationToken ct)
    {
        try
        {
            var tickets = await _database.TicketRepository.GetByTypeAsync(type, ct);
            var dtos = tickets.Select(entity => new TicketReportDTO(
                Id: entity.Id,
                Title: entity.Title,
                Text: entity.Text,
                Feedback: entity.Feedback ?? "",
                DepartmentGuid: entity.DepartmentId.ToString(),
                DepartmentName: entity.Department.Name,
                CreatedAt: entity.CreatedAt,
                Status: GetTicketStatus(entity.Status),
                AssigneeUsername: entity.Assignee?.Username ?? "",
                AssigneeFullName: entity.Assignee?.FullName ?? "",
                CreatorUsername: entity.Creator.Username,
                CreatorFullName: entity.Creator.FullName,
                ClosedAt: entity.ClosedAt,
                Type: GetTicketType(entity.Type)
            )).ToList();
            
            var fileContent = _excelReportService.GenerateTicketsReport(dtos, description);
            return Result<byte[]>.Success(fileContent);
        }
        catch (Exception e)
        {
            return Result<byte[]>.Failure($"Failed to generate {type} tickets report. Error: {e.Message}");
        }
    }

    private async Task<Result<byte[]>> CreateUsersReport(string? description, CancellationToken ct)
    {
        try
        {
            var users = await _database.UserRepository.GetAllAsync(ct);
            var dtos = users.Select(entity => new UserReportDTO(
                Id: entity.Id,
                FullName: entity.FullName,
                Username: entity.Username,
                Email: entity.Email,
                HiredAt: entity.HiredAt,
                FiredAt: entity.FiredAt,
                Birthday: entity.Birthday,
                Sex: GetUserSex(entity.IsMale),
                IsAdmin: entity.IsAdmin,
                HasPublishedRights: entity.HasPublishedRights,
                IsBlocked: entity.IsBlocked,
                DepartmentGuid: entity.DepartmentId?.ToString() ?? "",
                DepartmentName: entity.Department?.Name ?? "",
                PublishedPostsCount: entity.Posts.Count
            )).ToList();
            
            var fileContent = _excelReportService.GenerateUsersReport(dtos, description);
            return Result<byte[]>.Success(fileContent);
        }
        catch (Exception e)
        {
            return Result<byte[]>.Failure($"Failed to generate users report. Error: {e.Message}");
        }
    }
    
    private static string GetTicketStatus(TicketStatus status)
    {
        return status switch
        {
            TicketStatus.Created => "Создано",
            TicketStatus.InWork => "В работе",
            TicketStatus.Completed => "Выполнено",
            TicketStatus.Rejected => "Отклонено",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }
    
    private static string GetTicketType(TicketType type)
    {
        return type switch
        {
            TicketType.ProblemCase => "Заявка",
            TicketType.Suggestion => "Предложение",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
    
    private static string GetUserSex(bool isMale)
    {
        return isMale ? "Мужчина" : "Женщина";
    }
}