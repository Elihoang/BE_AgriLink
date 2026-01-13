using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Domain.Interface.IRepositories;

public interface ICropSeasonRepository : IRepository<CropSeason>
{
    Task<IEnumerable<CropSeason>> GetSeasonsByFarmIdAsync(Guid farmId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CropSeason>> GetActiveSeasonsAsync(CancellationToken cancellationToken = default);
    Task<CropSeason?> GetSeasonWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CropSeason>> GetSeasonsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
