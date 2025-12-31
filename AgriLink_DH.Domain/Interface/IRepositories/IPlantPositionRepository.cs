using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Domain.Interface.IRepositories;

public interface IPlantPositionRepository : IRepository<PlantPosition>
{
    // Query by Farm
    Task<IEnumerable<PlantPosition>> GetByFarmIdAsync(Guid farmId);
    Task<PlantPosition?> GetByPositionAsync(Guid farmId, int row, int col);
    Task<bool> PositionExistsAsync(Guid farmId, int row, int col);
    
    // Query by Season (optional)
    Task<IEnumerable<PlantPosition>> GetBySeasonIdAsync(Guid seasonId);
    Task<IEnumerable<PlantPosition>> GetByProductIdAsync(Guid seasonId, Guid productId);
    Task<Dictionary<string, int>> GetPlantSummaryAsync(Guid seasonId); // Key: ProductName, Value: Count
}
