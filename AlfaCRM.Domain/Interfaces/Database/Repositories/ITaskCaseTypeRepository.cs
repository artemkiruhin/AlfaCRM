using System.Collections;
using AlfaCRM.Domain.Interfaces.Database.Repositories.Base;
using AlfaCRM.Domain.Models.Entities;

namespace AlfaCRM.Domain.Interfaces.Database.Repositories;

public interface ITaskCaseTypeRepository : ICrudRepository<TaskCaseTypeEntity>
{
    Task<IEnumerable> GetAllByName(string name);
}