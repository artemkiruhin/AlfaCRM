using AlfaCRM.Domain.Interfaces.Database;
using AlfaCRM.Domain.Interfaces.Services.Entity;
using AlfaCRM.Domain.Models.DTOs;
using AlfaCRM.Domain.Models.Entities;
using DocumentFormat.OpenXml.Office2010.ExcelAc;

namespace AlfaCRM.Services.Entity;

public class LogService : ILogService
{
    private readonly IUnitOfWork _database;

    public LogService(IUnitOfWork database)
    {
        _database = database;
    }
    
    public async Task<Result<List<LogDTO>>> GetAll(CancellationToken ct)
    {
        try
        {
            var logs = await _database.LogRepository.GetAllAsync(ct);
            var dtos = logs.Select(log => new LogDTO(
                Id: log.Id,
                Message: log.Message,
                Type: GetLogTypeAsString(log.Type),
                UserIdString: log.UserId.HasValue ? log.UserId.Value.ToString() : "",
                Username: log.UserId.HasValue ? log.User.Username : "",
                CreatedAt: log.CreatedAt
            )).OrderByDescending(x => x.CreatedAt).ToList();

            return Result<List<LogDTO>>.Success(dtos);
        }
        catch (Exception e)
        {
            return Result<List<LogDTO>>.Failure($"Error while getting all logs: {e.Message}");
        }
    }

    private string GetLogTypeAsString(LogType logType)
    {
        return logType switch
        {
            LogType.Error => "Error",
            LogType.Warning => "Warning",
            LogType.Info => "Info",
            _ => throw new ArgumentOutOfRangeException(nameof(logType), logType, null)
        };
    }
}