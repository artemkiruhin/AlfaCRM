using System.Collections;
using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface ITaskCaseFastSolutionRepository : ICrudRepository<TaskCaseFastSolutionEntity>
{
    Task<IEnumerable> GetByType(Guid id);
}