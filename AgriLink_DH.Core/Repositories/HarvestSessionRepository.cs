using AgriLink_DH.Core.Configurations;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Core.Repositories;

public class HarvestSessionRepository : BaseRepository<HarvestSession>, IHarvestSessionRepository
{
    public HarvestSessionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<HarvestSession>> GetBySeasonIdAsync(Guid seasonId)
    {
        return await _dbSet
            .Include(h => h.CropSeason)
            .Include(h => h.HarvestBagDetails)
            .Where(h => h.SeasonId == seasonId)
            .OrderByDescending(h => h.HarvestDate)
            .ToListAsync();
    }

    public async Task<HarvestSession?> GetWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(h => h.CropSeason)
                .ThenInclude(cs => cs.Farm)
            .Include(h => h.HarvestBagDetails)
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<IEnumerable<HarvestSession>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(h => h.CropSeason)
                .ThenInclude(cs => cs.Farm)
            .Include(h => h.HarvestBagDetails)
            .Where(h => h.CropSeason.Farm.OwnerUserId == userId)
            .OrderByDescending(h => h.HarvestDate)
            .ToListAsync();
    }
}
