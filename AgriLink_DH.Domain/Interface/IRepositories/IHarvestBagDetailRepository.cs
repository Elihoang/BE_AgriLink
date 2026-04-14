using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Domain.Interface.IRepositories;

public interface IHarvestBagDetailRepository : IRepository<HarvestBagDetail>
{
    Task<IEnumerable<HarvestBagDetail>> GetBySessionIdAsync(Guid sessionId);
    Task<IEnumerable<HarvestBagDetail>> GetDraftsBySessionIdAsync(Guid sessionId);
}
