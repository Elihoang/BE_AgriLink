using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Domain.Interface.IRepositories;

public interface IDailyWorkLogRepository : IRepository<DailyWorkLog>
{
    Task<IEnumerable<DailyWorkLog>> GetBySeasonIdAsync(Guid seasonId);
    Task<IEnumerable<DailyWorkLog>> GetByDateRangeAsync(Guid seasonId, DateTime fromDate, DateTime toDate);
    Task<DailyWorkLog?> GetWithAssignmentsAsync(Guid id);
}
