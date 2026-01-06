using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Domain.Interface.IRepositories;

public interface IMaterialRepository : IRepository<Material>
{
    Task<IEnumerable<Material>> GetByUserIdAsync(Guid userId);
    Task<bool> ExistsByNameAndUserAsync(string name, Guid userId);
}
