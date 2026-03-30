using AgriLink_DH.Infrastructure.Data;
using AgriLink_DH.Domain.Interface.IRepositories;
using AgriLink_DH.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriLink_DH.Infrastructure.Repositories;

public class WeatherLogRepository : BaseRepository<WeatherLog>, IWeatherLogRepository
{
    public WeatherLogRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<WeatherLog>> GetByFarmIdAsync(Guid farmId)
    {
        return await _dbSet
            .Include(w => w.Farm)
            .Where(w => w.FarmId == farmId)
            .OrderByDescending(w => w.LogDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeatherLog>> GetByDateRangeAsync(Guid farmId, DateTime startDate, DateTime endDate)
    {
        return await _dbSet
            .Include(w => w.Farm)
            .Where(w => w.FarmId == farmId && w.LogDate >= startDate && w.LogDate <= endDate)
            .OrderBy(w => w.LogDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<WeatherLog>> GetByUserIdAsync(Guid userId)
    {
        return await _dbSet
            .Include(w => w.Farm)
            .Where(w => w.Farm.OwnerUserId == userId)
            .OrderByDescending(w => w.LogDate)
            .ToListAsync();
    }
}

