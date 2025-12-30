using AgriLink_DH.Core.Configurations;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Core.Repositories;

public class WorkerAdvanceRepository : BaseRepository<WorkerAdvance>, IWorkerAdvanceRepository
{
    public WorkerAdvanceRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<WorkerAdvance>> GetByWorkerIdAsync(Guid workerId)
    {
        return await _dbSet
            .Include(wa => wa.Worker)
            .Include(wa => wa.CropSeason)
            .Where(wa => wa.WorkerId == workerId)
            .OrderByDescending(wa => wa.AdvanceDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkerAdvance>> GetBySeasonIdAsync(Guid seasonId)
    {
        return await _dbSet
            .Include(wa => wa.Worker)
            .Include(wa => wa.CropSeason)
            .Where(wa => wa.SeasonId == seasonId)
            .OrderByDescending(wa => wa.AdvanceDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WorkerAdvance>> GetByWorkerAndSeasonAsync(Guid workerId, Guid seasonId)
    {
        return await _dbSet
            .Include(wa => wa.Worker)
            .Include(wa => wa.CropSeason)
            .Where(wa => wa.WorkerId == workerId && wa.SeasonId == seasonId)
            .OrderByDescending(wa => wa.AdvanceDate)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalAdvanceByWorkerAndSeasonAsync(Guid workerId, Guid seasonId)
    {
        return await _dbSet
            .Where(wa => wa.WorkerId == workerId && wa.SeasonId == seasonId)
            .SumAsync(wa => wa.Amount);
    }
}
