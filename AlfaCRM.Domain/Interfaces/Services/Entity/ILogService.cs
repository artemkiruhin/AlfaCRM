using AlfaCRM.Domain.Models.DTOs;

namespace AlfaCRM.Domain.Interfaces.Services.Entity;

public interface ILogService
{
    Task<Result<List<LogDTO>>> GetAll(CancellationToken ct);
}