using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Domain.Interface.IRepositories;

public interface ITaskTypeRepository : IRepository<TaskType>
{
    Task<IEnumerable<TaskType>> GetByFarmIdAsync(Guid farmId);
}
