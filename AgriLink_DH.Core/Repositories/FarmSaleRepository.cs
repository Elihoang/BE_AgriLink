using AgriLink_DH.Core.Configurations;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Core.Repositories;

public class FarmSaleRepository : BaseRepository<FarmSale>, IFarmSaleRepository
{
    public FarmSaleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<FarmSale>> GetBySeasonIdAsync(Guid seasonId)
    {
        return await _dbSet
            .Include(f => f.CropSeason)
            .Where(f => f.SeasonId == seasonId)
            .OrderByDescending(f => f.SaleDate)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalRevenueBySeasonAsync(Guid seasonId)
    {
        return await _dbSet
            .Where(f => f.SeasonId == seasonId)
            .SumAsync(f => f.TotalRevenue);
    }
}
