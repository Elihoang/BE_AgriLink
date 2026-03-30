using AgriLink_DH.Infrastructure.Data;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Infrastructure.Repositories;

public class PlantPositionRepository : BaseRepository<PlantPosition>, IPlantPositionRepository
{
    public PlantPositionRepository(ApplicationDbContext context) : base(context)
    {
    }

    // Override to eager load navigation properties
    public override async Task<PlantPosition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Product)
            .Include(p => p.Farm)
            .Include(p => p.CropSeason)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    // === Query by Farm (Primary) ===
    public async Task<IEnumerable<PlantPosition>> GetByFarmIdAsync(Guid farmId)
    {
        return await _dbSet
            .Include(p => p.Product)
            .Include(p => p.Farm)
            .Include(p => p.CropSeason)
            .Where(p => p.FarmId == farmId)
            .OrderBy(p => p.RowNumber)
            .ThenBy(p => p.ColumnNumber)
            .ToListAsync();
    }

    public async Task<PlantPosition?> GetByPositionAsync(Guid farmId, int row, int col)
    {
        return await _dbSet
            .Include(p => p.Product)
            .Include(p => p.Farm)
            .Include(p => p.CropSeason)
            .FirstOrDefaultAsync(p => p.FarmId == farmId 
                && p.RowNumber == row 
                && p.ColumnNumber == col);
    }

    public async Task<bool> PositionExistsAsync(Guid farmId, int row, int col)
    {
        return await _dbSet
            .AnyAsync(p => p.FarmId == farmId 
                && p.RowNumber == row 
                && p.ColumnNumber == col);
    }

    // === Query by Season (Secondary) ===
    public async Task<IEnumerable<PlantPosition>> GetBySeasonIdAsync(Guid seasonId)
    {
        return await _dbSet
            .Include(p => p.Product)
            .Include(p => p.Farm)
            .Include(p => p.CropSeason)
            .Where(p => p.SeasonId == seasonId)
            .OrderBy(p => p.RowNumber)
            .ThenBy(p => p.ColumnNumber)
            .ToListAsync();
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


}

