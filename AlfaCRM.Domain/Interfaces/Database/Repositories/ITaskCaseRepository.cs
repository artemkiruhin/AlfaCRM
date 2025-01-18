using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface ITaskCaseRepository : ICrudRepository<TaskCaseEntity>
{
    Task<IEnumerable<TaskCaseEntity>> GetByHelper(string username, CancellationToken ct);
    Task<IEnumerable<TaskCaseEntity>> GetByHelper(Guid id, CancellationToken ct);
    Task<IEnumerable<TaskCaseEntity>> GetByEmployee(string username, CancellationToken ct);
    Task<IEnumerable<TaskCaseEntity>> GetByEmployee(Guid id, CancellationToken ct);
    Task<IEnumerable<TaskCaseEntity>> GetByStatus(TaskCaseStatusEntity status, CancellationToken ct);
    Task<IEnumerable<TaskCaseEntity>> GetByCommentContent(string content, CancellationToken ct);
    Task<IEnumerable<TaskCaseEntity>> GetByType(Guid typeId, CancellationToken ct);
    Task<IEnumerable<TaskCaseEntity>> GetClosed(bool isClosed, CancellationToken ct);
    Task<IEnumerable<TaskCaseEntity>> GetByCreatedDate(DateTime from, DateTime to, CancellationToken ct);
    Task<IEnumerable<TaskCaseEntity>> GetByClosedDate(DateTime from, DateTime to, CancellationToken ct);
}