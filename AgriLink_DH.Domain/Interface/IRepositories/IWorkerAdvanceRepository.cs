using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Domain.Interface.IRepositories;

public interface IWorkerAdvanceRepository : IRepository<WorkerAdvance>
{
    Task<IEnumerable<WorkerAdvance>> GetByWorkerIdAsync(Guid workerId);
    Task<IEnumerable<WorkerAdvance>> GetBySeasonIdAsync(Guid seasonId);
    Task<IEnumerable<WorkerAdvance>> GetByWorkerAndSeasonAsync(Guid workerId, Guid seasonId);
    Task<decimal> GetTotalAdvanceByWorkerAndSeasonAsync(Guid workerId, Guid seasonId);
}
