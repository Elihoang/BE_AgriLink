using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Domain.Interface.IRepositories;

public interface IWeatherLogRepository : IRepository<WeatherLog>
{
    Task<IEnumerable<WeatherLog>> GetByFarmIdAsync(Guid farmId);
    Task<IEnumerable<WeatherLog>> GetByDateRangeAsync(Guid farmId, DateTime startDate, DateTime endDate);
    Task<IEnumerable<WeatherLog>> GetByUserIdAsync(Guid userId);
}
