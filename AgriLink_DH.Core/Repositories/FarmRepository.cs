using AgriLink_DH.Core.Configurations;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Core.Repositories;

public class FarmRepository : BaseRepository<Farm>, IFarmRepository
{
    public FarmRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Farm>> GetFarmsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(f => f.OwnerUserId == userId)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAndUserAsync(string name, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(f => f.Name == name && f.OwnerUserId == userId, cancellationToken);
    }
}
