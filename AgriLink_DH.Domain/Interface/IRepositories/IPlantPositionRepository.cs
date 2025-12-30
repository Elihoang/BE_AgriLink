using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Domain.Interface.IRepositories;

public interface IPlantPositionRepository : IRepository<PlantPosition>
{
    Task<IEnumerable<PlantPosition>> GetBySeasonIdAsync(Guid seasonId);
    Task<PlantPosition?> GetByPositionAsync(Guid seasonId, int row, int col);
    Task<IEnumerable<PlantPosition>> GetByProductIdAsync(Guid seasonId, Guid productId);
    Task<Dictionary<string, int>> GetPlantSummaryAsync(Guid seasonId); // Key: ProductName, Value: Count
    Task<bool> PositionExistsAsync(Guid seasonId, int row, int col);
}
