using AgriLink_DH.Core.Configurations;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Core.Repositories;

public class HarvestBagDetailRepository : BaseRepository<HarvestBagDetail>, IHarvestBagDetailRepository
{
    public HarvestBagDetailRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<HarvestBagDetail>> GetBySessionIdAsync(Guid sessionId)
    {
        return await _dbSet
            .Where(b => b.SessionId == sessionId)
            .OrderBy(b => b.BagIndex)
            .ToListAsync();
    }
}
