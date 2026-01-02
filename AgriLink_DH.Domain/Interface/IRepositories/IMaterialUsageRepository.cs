using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Domain.Interface.IRepositories;

public interface IMaterialUsageRepository : IRepository<MaterialUsage>
{
    Task<IEnumerable<MaterialUsage>> GetBySeasonIdAsync(Guid seasonId);
    Task<IEnumerable<MaterialUsage>> GetByFarmIdAsync(Guid farmId);
    Task<decimal> GetTotalCostBySeasonAsync(Guid seasonId);
}
