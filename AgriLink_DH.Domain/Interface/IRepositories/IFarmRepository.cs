using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Domain.Interface.IRepositories;

public interface IFarmRepository : IRepository<Farm>
{
    Task<IEnumerable<Farm>> GetFarmsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAndUserAsync(string name, Guid userId, CancellationToken cancellationToken = default);
    Task<Farm?> GetFarmWithDetailsByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
