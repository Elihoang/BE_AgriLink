using AgriLink_DH.Core.Configurations;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Core.Repositories;

public class WorkAssignmentRepository : BaseRepository<WorkAssignment>, IWorkAssignmentRepository
{
    public WorkAssignmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<WorkAssignment>> GetByLogIdAsync(Guid logId)
    {
        return await _dbSet
            .Include(wa => wa.Worker)
            .Where(wa => wa.LogId == logId)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkAssignment>> GetByWorkerIdAsync(Guid workerId, DateTime fromDate, DateTime toDate)
    {
        return await _dbSet
            .Include(wa => wa.DailyWorkLog)
            .Where(wa => wa.WorkerId == workerId && wa.DailyWorkLog.WorkDate >= fromDate && wa.DailyWorkLog.WorkDate <= toDate)
            .OrderBy(wa => wa.DailyWorkLog.WorkDate)
            .ToListAsync();
    }
}
