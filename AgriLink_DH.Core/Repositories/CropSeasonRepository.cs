using AgriLink_DH.Core.Configurations;
using AgriLink_DH.Domain.Common;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Core.Repositories;

public class CropSeasonRepository : BaseRepository<CropSeason>, ICropSeasonRepository
{
    public CropSeasonRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CropSeason>> GetSeasonsByFarmIdAsync(Guid farmId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(cs => cs.Product)
            .Include(cs => cs.Farm)
            .Where(cs => cs.FarmId == farmId)
            .OrderByDescending(cs => cs.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CropSeason>> GetActiveSeasonsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(cs => cs.Product)
            .Include(cs => cs.Farm)
            .Where(cs => cs.Status == SeasonStatus.Active)
            .OrderByDescending(cs => cs.StartDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<CropSeason?> GetSeasonWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(cs => cs.Product)
            .Include(cs => cs.Farm)
            .FirstOrDefaultAsync(cs => cs.Id == id, cancellationToken);
    }
}
