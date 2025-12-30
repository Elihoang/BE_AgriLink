using AgriLink_DH.Core.Configurations;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Core.Repositories;

public class MaterialUsageRepository : BaseRepository<MaterialUsage>, IMaterialUsageRepository
{
    public MaterialUsageRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MaterialUsage>> GetBySeasonIdAsync(Guid seasonId)
    {
        return await _dbSet
            .Include(m => m.CropSeason)
            .Where(m => m.SeasonId == seasonId && !m.IsDeleted)
            .OrderByDescending(m => m.UsageDate)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalCostBySeasonAsync(Guid seasonId)
    {
        return await _dbSet
            .Where(m => m.SeasonId == seasonId && !m.IsDeleted)
            .SumAsync(m => m.TotalCost);
    }
}
