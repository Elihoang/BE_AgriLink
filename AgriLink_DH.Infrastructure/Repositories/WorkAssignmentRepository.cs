using AgriLink_DH.Infrastructure.Data;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Infrastructure.Repositories;

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

    public async Task<IEnumerable<Worker>> GetWorkersBySeasonIdAsync(Guid seasonId)
    {
        return await _dbSet
            .Include(wa => wa.DailyWorkLog)
            .Include(wa => wa.Worker)
            .Where(wa => wa.DailyWorkLog.SeasonId == seasonId)
            .Select(wa => wa.Worker)
            .Distinct()
            .ToListAsync();
    }

    public async Task<IEnumerable<Worker>> GetWorkersByFarmIdAsync(Guid farmId)
    {
        return await _dbSet
            .Include(wa => wa.DailyWorkLog)
            .ThenInclude(dwl => dwl.CropSeason)
            .Include(wa => wa.Worker)
            .Where(wa => wa.DailyWorkLog.CropSeason.FarmId == farmId)
            .Select(wa => wa.Worker)
            .Distinct()
            .ToListAsync();
    }
}

