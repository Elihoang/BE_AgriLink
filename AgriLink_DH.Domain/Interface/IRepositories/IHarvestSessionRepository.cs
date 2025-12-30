using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Domain.Interface.IRepositories;

public interface IHarvestSessionRepository : IRepository<HarvestSession>
{
    Task<IEnumerable<HarvestSession>> GetBySeasonIdAsync(Guid seasonId);
    Task<HarvestSession?> GetWithDetailsAsync(Guid id);
}
