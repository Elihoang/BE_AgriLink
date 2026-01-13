using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Domain.Interface.IRepositories;

public interface IDailyWorkLogRepository : IRepository<DailyWorkLog>
{
    Task<IEnumerable<DailyWorkLog>> GetBySeasonIdAsync(Guid seasonId);
    Task<IEnumerable<DailyWorkLog>> GetByTaskTypeIdAsync(Guid taskTypeId);
    Task<IEnumerable<DailyWorkLog>> GetByDateRangeAsync(Guid seasonId, DateTime fromDate, DateTime toDate);
    Task<DailyWorkLog?> GetWithAssignmentsAsync(Guid id);
    Task<IEnumerable<DailyWorkLog>> GetByFarmAndTaskTypeAsync(Guid farmId, Guid taskTypeId);
    Task<IEnumerable<DailyWorkLog>> GetBySeasonAndTaskTypeAsync(Guid seasonId, Guid taskTypeId);
    Task<IEnumerable<DailyWorkLog>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<DailyWorkLog>> GetByFarmAndDateRangeAsync(Guid farmId, DateTime fromDate, DateTime toDate);
}
