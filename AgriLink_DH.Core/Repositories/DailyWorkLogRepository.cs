using AgriLink_DH.Core.Configurations;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Core.Repositories;

public class DailyWorkLogRepository : BaseRepository<DailyWorkLog>, IDailyWorkLogRepository
{
    public DailyWorkLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<DailyWorkLog>> GetBySeasonIdAsync(Guid seasonId)
    {
        return await _dbSet
            .Include(d => d.TaskType)
            .Include(d => d.WorkAssignments)
                .ThenInclude(wa => wa.Worker)
            .Where(d => d.SeasonId == seasonId)
            .OrderByDescending(d => d.WorkDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<DailyWorkLog>> GetByTaskTypeIdAsync(Guid taskTypeId)
    {
        return await _dbSet
            .Include(d => d.CropSeason)
                .ThenInclude(s => s.Product)
            .Include(d => d.TaskType)
            .Include(d => d.WorkAssignments)
                .ThenInclude(wa => wa.Worker)
            .Where(d => d.TaskTypeId == taskTypeId)
            .OrderByDescending(d => d.WorkDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<DailyWorkLog>> GetByDateRangeAsync(Guid seasonId, DateTime fromDate, DateTime toDate)
    {
        return await _dbSet
            .Where(d => d.SeasonId == seasonId && d.WorkDate >= fromDate && d.WorkDate <= toDate)
            .OrderBy(d => d.WorkDate)
            .ToListAsync();
    }

    public async Task<DailyWorkLog?> GetWithAssignmentsAsync(Guid id)
    {
        return await _dbSet
            .Include(d => d.TaskType)
            .Include(d => d.WorkAssignments)
                .ThenInclude(wa => wa.Worker)
            .FirstOrDefaultAsync(d => d.Id == id);
    }
}
