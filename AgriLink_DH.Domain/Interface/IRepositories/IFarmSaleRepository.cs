using AgriLink_DH.Domain.Models;

namespace AgriLink_DH.Domain.Interface.IRepositories;

public interface IFarmSaleRepository : IRepository<FarmSale>
{
    Task<IEnumerable<FarmSale>> GetBySeasonIdAsync(Guid seasonId);
    Task<decimal> GetTotalRevenueBySeasonAsync(Guid seasonId);
}
