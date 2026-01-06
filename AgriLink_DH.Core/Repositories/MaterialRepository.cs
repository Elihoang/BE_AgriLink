using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using AgriLink_DH.Core.Configurations; // Fix namespace for ApplicationDbContext
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Core.Repositories;

public class MaterialRepository : BaseRepository<Material>, IMaterialRepository // Fix inheritance
{
    // _context and _dbSet are inherited from BaseRepository

    public MaterialRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Material>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Where(m => m.OwnerUserId == userId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> ExistsByNameAndUserAsync(string name, Guid userId)
    {
        return await _dbSet.AnyAsync(m => m.Name.ToLower() == name.ToLower() && m.OwnerUserId == userId);
    }
}
