using AgriLink_DH.Core.Configurations;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Core.Repositories;

public class PlantPositionRepository : BaseRepository<PlantPosition>, IPlantPositionRepository
{
    public PlantPositionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PlantPosition>> GetBySeasonIdAsync(Guid seasonId)
    {
        return await _dbSet
            .Include(p => p.Product) // Include Product info
            .Where(p => p.SeasonId == seasonId)
            .OrderBy(p => p.RowNumber)
            .ThenBy(p => p.ColumnNumber)
            .ToListAsync();
    }

    public async Task<PlantPosition?> GetByPositionAsync(Guid seasonId, int row, int col)
    {
        return await _dbSet
            .Include(p => p.Product)
            .FirstOrDefaultAsync(p => p.SeasonId == seasonId 
                && p.RowNumber == row 
                && p.ColumnNumber == col);
    }

    public async Task<IEnumerable<PlantPosition>> GetByProductIdAsync(Guid seasonId, Guid productId)
    {
        return await _dbSet
            .Include(p => p.Product)
            .Where(p => p.SeasonId == seasonId && p.ProductId == productId)
            .OrderBy(p => p.RowNumber)
            .ThenBy(p => p.ColumnNumber)
            .ToListAsync();
    }

    public async Task<Dictionary<string, int>> GetPlantSummaryAsync(Guid seasonId)
    {
        return await _dbSet
            .Include(p => p.Product)
            .Where(p => p.SeasonId == seasonId)
            .GroupBy(p => p.Product.Name)
            .Select(g => new { ProductName = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ProductName, x => x.Count);
    }

    public async Task<bool> PositionExistsAsync(Guid seasonId, int row, int col)
    {
        return await _dbSet
            .AnyAsync(p => p.SeasonId == seasonId 
                && p.RowNumber == row 
                && p.ColumnNumber == col);
    }
}
